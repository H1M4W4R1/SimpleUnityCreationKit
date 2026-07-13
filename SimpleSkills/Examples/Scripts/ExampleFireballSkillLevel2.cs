namespace Systems.SimpleSkills.Examples.Scripts
{
    /// <summary>
    ///     Level 2 fireball variant: 25 damage, 2.5s cooldown.
    /// </summary>
    public sealed class ExampleFireballSkillLevel2 : ExampleFireballSkill
    {
        public override int Level => 2;

        protected override int Damage => 25;

        public override float CooldownTime => 2.5f;
    }
}
