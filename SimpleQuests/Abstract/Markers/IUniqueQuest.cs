namespace Systems.SimpleQuests.Abstract.Markers
{
    /// <summary>
    ///     Marker interface that restricts a quest to a single active instance at a time.
    ///     When a quest implementing this interface is started via
    ///     <see cref="Systems.SimpleQuests.Utility.QuestAPI.TryStartQuest{TQuest}"/>,
    ///     the attempt will fail with <see cref="Systems.SimpleQuests.Operations.QuestOperations.QuestAlreadyStarted"/>
    ///     if an active instance of the same type already exists.
    /// </summary>
    /// <example>
    ///     <code>
    ///     public class MyUniqueQuest : Quest, IUniqueQuest
    ///     {
    ///         // Only one active instance of this quest is allowed at a time
    ///     }
    ///     </code>
    /// </example>
    public interface IUniqueQuest { }
}
