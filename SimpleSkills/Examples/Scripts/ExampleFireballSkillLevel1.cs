namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Level 1 fireball variant: 10 damage, 3s cooldown.
    /// </summary>
    public sealed class ExampleFireballSkillLevel1 : ExampleFireballSkill
    {
        public override int Level => 1;

        protected override int Damage => 10;

        public override float CooldownTime => 3f;
    }
}
