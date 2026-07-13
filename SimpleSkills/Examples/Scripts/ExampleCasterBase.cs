using Systems.SimpleCore.Examples;
using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Components;
using Systems.SimpleSkills.Data.Abstract;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Systems.SimpleSkills.Examples.Scripts
{
    public sealed class ExampleCasterBase : SkillCasterBase
    {
        [SerializeField] [FormerlySerializedAs("fireballLevel")] private int _fireballLevel = 1;
        [SerializeField] private bool _createRuntimeUI = true;

        private ExampleRuntimePanel _panel;
        private string _lastResult = "none";

        private void Start()
        {
            if (_createRuntimeUI)
            {
                CreateRuntimeUI();
            }

            RefreshStatus("Ready. Cast skills from the runtime controls.");
        }

        /// <summary>
        ///     Override to drive fireball level from the serialized field.
        ///     Any other leveled skill falls back to the default (skill's own Level property).
        /// </summary>
        protected override int GetSkillLevel(ISkillWithLevels skill)
        {
            if (skill is ExampleFireballSkill)
            {
                return _fireballLevel;
            }

            return base.GetSkillLevel(skill);
        }

        [ContextMenu("Cast channeling skill")]
        public void CastChannelingSkill()
        {
            OperationResult result = TryCastSkill<ExampleIChannelingSkill>();
            SetLastResult(result, "Channeling cast attempted.");
        }

        [ContextMenu("Cancel channeling skill")]
        public void CancelChannelingSkill()
        {
            OperationResult result = TryCancelSkill<ExampleIChannelingSkill>();
            if (OperationResult.IsError(result))
            {
                Debug.LogError("Failed to cancel channeling skill");
            }
            else
            {
                Debug.Log("Channeling skill cancelled");
            }

            SetLastResult(result, "Channeling cancel attempted.");
        }

        [ContextMenu("Interrupt channeling skill")]
        public void InterruptChannelingSkill()
        {
            OperationResult result = TryInterruptSkill<ExampleIChannelingSkill>(null);
            if (OperationResult.IsError(result))
            {
                Debug.LogError("Failed to interrupt channeling skill");
            }
            else
            {
                Debug.Log("Channeling skill interrupted");
            }

            SetLastResult(result, "Channeling interrupt attempted.");
        }

        [ContextMenu("Cast one-time skill")]
        public void CastRegularSkill()
        {
            OperationResult result = TryCastSkill<ExampleOneTimeSkill>();
            SetLastResult(result, "One-time skill cast attempted.");
        }

        [ContextMenu("Cast dash skill (charges)")]
        public void CastDashSkill()
        {
            OperationResult result = TryCastSkill<ExampleDashSkill>();
            SetLastResult(result, "Dash skill cast attempted.");
        }

        [ContextMenu("Cast health potion (skill group)")]
        public void CastHealthPotion()
        {
            OperationResult result = TryCastSkill<ExampleHealthPotionSkill>();
            SetLastResult(result, "Health potion cast attempted.");
        }

        [ContextMenu("Cast mushroom (skill group)")]
        public void CastMushroom()
        {
            OperationResult result = TryCastSkill<ExampleMushroomSkill>();
            SetLastResult(result, "Mushroom cast attempted.");
        }

        /// <summary>
        ///     Casts the fireball at the level configured in <see cref="_fireballLevel"/>.
        ///     Any level-1 variant is used as the entry point; the system resolves the correct asset.
        /// </summary>
        [ContextMenu("Cast fireball (leveled skill)")]
        public void CastFireball()
        {
            OperationResult result = TryCastSkill<ExampleFireballSkillLevel1>();
            SetLastResult(result, "Fireball cast attempted.");
        }

        /// <summary>
        ///     Toggles the regeneration aura on/off.
        ///     First call activates it; casting again while active deactivates it.
        /// </summary>
        [ContextMenu("Toggle regeneration aura (activated skill)")]
        public void ToggleRegenerationAura()
        {
            OperationResult result = TryCastSkill<ExampleRegenerationAuraSkill>();
            SetLastResult(result, "Regeneration aura toggle attempted.");
        }

        private void CreateRuntimeUI()
        {
            _panel = ExampleRuntimePanel.Create(
                "SimpleSkills Example",
                "Navigate one-time, charged, channeled, grouped, leveled, and activated skill cases.");

            _panel.AddSection("Skill Casts");
            Button regularButton = _panel.AddButton("Cast One-Time Skill");
            regularButton.onClick.AddListener(CastRegularSkill);

            Button fireballButton = _panel.AddButton("Cast Fireball");
            fireballButton.onClick.AddListener(CastFireball);

            Button dashButton = _panel.AddButton("Cast Dash");
            dashButton.onClick.AddListener(CastDashSkill);

            Button auraButton = _panel.AddButton("Toggle Regeneration Aura");
            auraButton.onClick.AddListener(ToggleRegenerationAura);

            _panel.AddSection("Channeling");
            Button channelButton = _panel.AddButton("Cast Channeling Skill");
            channelButton.onClick.AddListener(CastChannelingSkill);

            Button cancelButton = _panel.AddButton("Cancel Channeling");
            cancelButton.onClick.AddListener(CancelChannelingSkill);

            Button interruptButton = _panel.AddButton("Interrupt Channeling");
            interruptButton.onClick.AddListener(InterruptChannelingSkill);

            _panel.AddSection("Shared Cooldown");
            Button healthPotionButton = _panel.AddButton("Cast Health Potion");
            healthPotionButton.onClick.AddListener(CastHealthPotion);

            Button mushroomButton = _panel.AddButton("Cast Mushroom");
            mushroomButton.onClick.AddListener(CastMushroom);
        }

        private void SetLastResult(in OperationResult result, string message)
        {
            _lastResult = ExampleRuntimePanel.FormatResult(result);
            RefreshStatus(message);
        }

        private void RefreshStatus(string message)
        {
            if (ReferenceEquals(_panel, null))
            {
                return;
            }

            _panel.SetStatus(
                message +
                "\nFireball level: " + _fireballLevel +
                " | Dash charges: " + GetAvailableCharges<ExampleDashSkill>() +
                " | Aura active: " + IsSkillActivated<ExampleRegenerationAuraSkill>() +
                "\nLast result: " + _lastResult);
        }
    }
}
