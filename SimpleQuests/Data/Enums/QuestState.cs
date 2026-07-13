namespace Systems.SimpleQuests.Data.Enums
{
    public enum QuestState
    {
        /// <summary>
        ///     Should not be visible to the player
        /// </summary>
        Hidden,
        
        /// <summary>
        ///     Should be visible to the player, but grayed out
        /// </summary>
        Inactive,
        
        /// <summary>
        ///     Should be visible to the player and active
        /// </summary>
        InProgress,
        
        /// <summary>
        ///     Should be visible to the player and marked as completed (or removed entirely)
        /// </summary>
        Completed,
        
        /// <summary>
        ///     Should be visible to player and marked as failed (or removed entirely) 
        /// </summary>
        Failed
    }
}