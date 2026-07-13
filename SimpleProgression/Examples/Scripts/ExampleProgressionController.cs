using Systems.SimpleProgression.Components;
using UnityEngine;

namespace Systems.SimpleProgression.Examples.Scripts
{
    public sealed class ExampleProgressionController : LevelControllerBase
    {
        public int CurrentExampleLevel => GetCurrentLevel();

        public override int GetMaxLevel()
        {
            return 5;
        }

        protected override ulong GetExperienceForLevel(int level)
        {
            return level <= 0 ? 0UL : (ulong)(level * level * 100);
        }

        protected override void OnLevelIncreased(int newLevel)
        {
            Debug.Log("[SimpleProgression] Reached level " + newLevel);
        }

        protected override void OnExperienceChanged(ulong previousExperience, ulong newExperience)
        {
            Debug.Log("[SimpleProgression] Experience changed from " + previousExperience + " to " + newExperience);
        }
    }
}
