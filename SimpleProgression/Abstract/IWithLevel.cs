namespace Systems.SimpleProgression.Abstract
{
    /// <summary>
    ///     Represents object that has level
    /// </summary>
    public interface IWithLevel : IWithExperience
    {
        /// <summary>
        ///     Get current level based on this object's experience
        /// </summary>
        public int GetCurrentLevel();
    }
}