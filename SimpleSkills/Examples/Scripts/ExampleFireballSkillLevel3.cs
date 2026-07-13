namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Level 3 fireball variant: 50 damage, 2s cooldown.
    /// </summary>
    public sealed class ExampleFireballSkillLevel3 : ExampleFireballSkill
    {
        public override int Level => 3;

        protected override int Damage => 50;

        public override float CooldownTime => 2f;
    }
}
