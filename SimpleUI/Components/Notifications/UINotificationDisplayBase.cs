using JetBrains.Annotations;
using Systems.SimpleUI.Components.Lists;
using Systems.SimpleUI.Context.Notifications;

namespace Systems.SimpleUI.Components.Notifications
{
    /// <summary>
    ///     Notification display component used to display <see cref="NotificationBase"/>
    ///     By default it's just a wrapper for <see cref="UIListBase{TListObject}"/>
    /// </summary>
    /// <remarks>
    ///     Context provided to this component should contain notifications to display
    ///     sorted by <see cref="NotificationBase.Priority"/> in ascending order.
    /// </remarks>
    public abstract class UINotificationDisplayBase : UIListBase<NotificationBase>
    {
        protected virtual void OnNotificationShown([NotNull] NotificationBase notification)
        {
            
        }

        protected virtual void OnNotificationHidden([NotNull] NotificationBase notification)
        {
            
        }
        
        protected sealed override void OnElementHidden(UIListElementBase<NotificationBase> element)
        {
            base.OnElementHidden(element);
            
            // Notify about hidden notification, context is invalid so we access the cache
            // as it should be still kept as correct value
            if (element.CachedContext != null) OnNotificationHidden(element.CachedContext);
        }

        protected sealed override void OnElementShown(UIListElementBase<NotificationBase> element)
        {
            base.OnElementShown(element);
            
            // Notify about shown notification, context should be available
            OnNotificationShown(element.Context!);
        }
    }
}