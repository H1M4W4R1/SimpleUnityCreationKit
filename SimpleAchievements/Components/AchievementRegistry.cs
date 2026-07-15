using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Systems.SimpleAchievements.Abstract;
using Systems.SimpleAchievements.Data.Databases;
using Systems.SimpleAchievements.Data.SaveFiles;
using Systems.SimpleAchievements.Data.Settings;
using Systems.SimpleAchievements.Operations;
using Systems.SimpleAchievements.Structs;
using Systems.SimpleAchievements.Utility;
using Systems.SimpleCore.Behaviours;
using Systems.SimpleCore.Behaviours.Markers;
using Systems.SimpleCore.Operations;
using Systems.SimpleSaving.Abstract;
using Systems.SimpleCore.Storage.Lists;
using Systems.SimpleIntegration.Abstract.Features;
using Systems.SimpleIntegration.Utility;
using UnityEngine;

namespace Systems.SimpleAchievements.Components
{
    /// <summary>
    ///     Runtime manager for all achievements. Created automatically at startup as a
    ///     <c>DontDestroyOnLoad</c> singleton. Polls condition-based achievements each tick,
    ///     propagates unlocks to registered platforms, and persists unlock state to disk.
    /// </summary>
    /// <remarks>
    ///     Do not add this component manually. Use <see cref="AchievementAPI"/> for all
    ///     external interactions.
    /// </remarks>
    public sealed class AchievementRegistry : SimpleBehaviour, ISaveData<AchievementSaveFile>, IUniqueBehaviour,
        IPersistentBehaviour, IAwakeBehaviour, ITickableBehaviour, IDestroyBehaviour
    {
        private static AchievementRegistry _instance;

        /// <summary>The active registry singleton.</summary>
        [NotNull]
        public static AchievementRegistry Instance
        {
            get
            {
                EnsureExists();
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance) return;

            _instance = null;
            GameObject gameObject = new GameObject("[AchievementRegistry]");
            _instance = gameObject.AddComponent<AchievementRegistry>();
        }

        private HashSet<string> _unlockedIds;
        private AchievementData[] _conditionalAchievements;
        private bool _isInitialized;

        protected override void OnBehaviourAwake()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _instance    = this;
            _unlockedIds = new HashSet<string>();

            BuildConditionalCache();
            IntegrationAPI.IsAvailable<IAchievementPlatform>();
            LoadFromDisk();
        }

#if UNITY_INCLUDE_TESTS
        internal void AwakeForTests()
        {
            OnBehaviourAwake();
        }

        internal void ShutdownForTests()
        {
            OnBehaviourDestroyed();
        }
#endif

        protected override void OnBehaviourDestroyed()
        {
            if (ReferenceEquals(_instance, this))
                _instance = null;
        }

        // ------------------------------------------------------------------ //
        //  Cache construction                                                  //
        // ------------------------------------------------------------------ //

        private void BuildConditionalCache()
        {
            ROListAccess<AchievementData> access = AchievementDatabase.GetAllAchievements();
            IReadOnlyList<AchievementData> list  = access.List;

            int conditionalCount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                AchievementData achievement = list[i];
                if (!achievement) continue;
                if (achievement.IsConditional) conditionalCount++;
            }

            _conditionalAchievements = new AchievementData[conditionalCount];
            int index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                AchievementData achievement = list[i];
                if (!achievement) continue;
                if (achievement.IsConditional) _conditionalAchievements[index++] = achievement;
            }

            access.Release();
        }

        // ------------------------------------------------------------------ //
        //  Tick - condition monitoring                                         //
        // ------------------------------------------------------------------ //

        protected override void OnTick(float deltaTimeSeconds)
        {
            if (ReferenceEquals(_conditionalAchievements, null)) return;

            for (int i = 0; i < _conditionalAchievements.Length; i++)
            {
                AchievementData achievement = _conditionalAchievements[i];
                if (!achievement) continue;
                if (_unlockedIds.Contains(achievement.PlatformId)) continue;
                if (achievement.CheckCondition()) UnlockInternal(achievement);
            }
        }

#if UNITY_INCLUDE_TESTS
        internal void TickForTests(float deltaTimeSeconds)
        {
            OnTick(deltaTimeSeconds);
        }
#endif

        // ------------------------------------------------------------------ //
        //  Unlock logic                                                        //
        // ------------------------------------------------------------------ //

        internal OperationResult Unlock(in AchievementUnlockContext context)
        {
            AchievementData achievement = context.Achievement;
            if (ReferenceEquals(achievement, null) || !achievement)
                return AchievementOperations.InvalidAchievement();

            if (_unlockedIds.Contains(achievement.PlatformId))
                return AchievementOperations.AlreadyUnlocked();

            OperationResult validationResult = achievement.CanUnlockInternal(in context);
            if (!validationResult) return validationResult;

            UnlockInternal(achievement);
            return AchievementOperations.Unlocked();
        }

        internal OperationResult NotifyProgress([CanBeNull] AchievementData achievement)
        {
            if (ReferenceEquals(achievement, null) || !achievement)
                return AchievementOperations.InvalidAchievement();

            if (_unlockedIds.Contains(achievement.PlatformId))
                return AchievementOperations.AlreadyUnlocked();

            if (!(achievement is IProgressibleAchievement progressibleAchievement))
                return AchievementOperations.NotProgressible();

            if (!progressibleAchievement.UpdateProgress())
                return AchievementOperations.ProgressUpdated();

            AchievementUnlockContext context = new AchievementUnlockContext(achievement, true);
            return Unlock(in context);
        }

        private void UnlockInternal([NotNull] AchievementData achievement)
        {
            _unlockedIds.Add(achievement.PlatformId);
            achievement.NotifyUnlocked();

            ROListAccess<IAchievementPlatform> platforms =
                IntegrationAPI.GetAvailable<IAchievementPlatform>();
            for (int i = 0; i < platforms.List.Count; i++)
            {
                IAchievementPlatform platform = platforms.List[i];
                platform.UnlockAchievement(achievement.PlatformId);
            }

            platforms.Release();

            if (AchievementsSettings.Instance.AutoSaveOnUnlock)
                PersistToDisk();
        }

        internal bool IsUnlocked([CanBeNull] string platformId) =>
            !string.IsNullOrWhiteSpace(platformId) && _unlockedIds.Contains(platformId);

        // ------------------------------------------------------------------ //
        //  Automatic disk persistence                                          //
        //  SaveAPI handles in-memory format conversion; disk I/O lives here.  //
        // ------------------------------------------------------------------ //

        internal void PersistToDisk()
        {
            AchievementSaveFile file = BuildSaveFile();
            string json = JsonUtility.ToJson(file);
            string path = Path.Combine(
                Application.persistentDataPath,
                AchievementsSettings.Instance.SaveFileName);
            File.WriteAllText(path, json);
        }

        internal void LoadFromDisk()
        {
            string path = Path.Combine(
                Application.persistentDataPath,
                AchievementsSettings.Instance.SaveFileName);

            if (!File.Exists(path)) return;

            string json = File.ReadAllText(path);
            AchievementSaveFile file = JsonUtility.FromJson<AchievementSaveFile>(json);
            if (!ReferenceEquals(file, null)) ParseSaveFile(file);
        }

        // ------------------------------------------------------------------ //
        //  ISaveData<AchievementSaveFile>                                      //
        //  Implement for optional host-game save system integration via         //
        //  AchievementAPI.SaveToMemory() / AchievementAPI.Load(SaveFileBase).  //
        // ------------------------------------------------------------------ //

        /// <inheritdoc />
        /// <remarks>State is always live in <c>_unlockedIds</c> - nothing to collect.</remarks>
        public void CollectData() { }

        /// <inheritdoc />
        public AchievementSaveFile BuildSaveFile()
        {
            string[] ids = new string[_unlockedIds.Count];
            _unlockedIds.CopyTo(ids);
            AchievementSaveFile file = new AchievementSaveFile();
            file.UnlockedPlatformIds = ids;
            return file;
        }

        /// <inheritdoc />
        public void ParseSaveFile(AchievementSaveFile saveFile)
        {
            _unlockedIds.Clear();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ReferenceEquals(saveFile?.UnlockedPlatformIds, null)) return;

            ReadOnlySpan<string> ids = saveFile.UnlockedPlatformIds.AsSpan();
            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i];
                if (!string.IsNullOrWhiteSpace(id)) _unlockedIds.Add(id);
            }
        }

        /// <inheritdoc />
        /// <remarks><c>_unlockedIds</c> is already live after <see cref="ParseSaveFile"/> - nothing to distribute.</remarks>
        public void DistributeData() { }
    }
}
