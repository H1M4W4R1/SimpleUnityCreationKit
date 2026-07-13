using Systems.SimpleCore.Operations;
using Systems.SimpleSkills.Data.Abstract;
using Systems.SimpleSkills.Data.Context;
using Systems.SimpleSkills.Operations;
using UnityEngine;

namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Abstract base for a leveled fireball skill.
    ///     Each level variant is a separate ScriptableObject asset in the database.
    ///     The caster resolves the correct level at cast-time via <c>SkillCasterBase.GetSkillLevel</c>.
    /// </summary>
    public abstract class ExampleFireballSkill : SkillWithLevels<ExampleFireballSkill>
    {
        /// <summary>
        ///     Abstract — each level variant must declare its own level.
        /// </summary>
        public abstract override int Level { get; }

        /// <summary>
        ///     Damage output for this level variant.
        /// </summary>
        protected abstract int Damage { get; }

        public override float ChargingTime => 0.5f;

        protected internal override void OnCastStarted(in CastSkillContext context)
        {
            base.OnCastStarted(in context);
            Debug.Log($"Skill {name} (level {Level}) — fireball launched by {context.caster.name}");
        }

        protected internal override void OnCastEnded(in CastSkillContext context)
        {
            base.OnCastEnded(in context);
            Debug.Log($"Skill {name} (level {Level}) — fireball hit for {Damage} damage on {context.caster.name}");
        }

        protected internal override void OnCastFailed(in CastSkillContext context, in OperationResult reason)
        {
            base.OnCastFailed(in context, in reason);
            if (OperationResult.AreSimilar(reason, SkillOperations.CooldownNotFinished()))
                Debug.LogError($"Skill {name} — cooldown not finished for {context.caster.name}");
        }
    }

}
