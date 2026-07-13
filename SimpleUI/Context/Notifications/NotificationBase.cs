using Systems.SimpleUI.Components.Notifications;

namespace Systems.SimpleUI.Context.Notifications
{
    /// <summary>
    ///     Basic UI notification used by <see cref="UINotificationDisplayBase"/>
    /// </summary>
    public abstract class NotificationBase
    {
        /// <summary>
        ///     Priority of the notification, lower value means higher priority
        /// </summary>
        protected int Priority { get; set; }
        
    }
}