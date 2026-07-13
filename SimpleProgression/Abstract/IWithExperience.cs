namespace Systems.SimpleProgression.Abstract
{
    /// <summary>
    ///     Represents object that has experience (e.g. skill or player)
    /// </summary>
    public interface IWithExperience
    {
        public ulong Experience { get; }
    }
}