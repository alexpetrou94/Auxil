namespace Auxil.Common.Notifications;

/// <summary>Describes why an active notification was dismissed.</summary>
[PublicAPI]
public enum NotificationDismissReason
{
    /// <summary>The notification has not been dismissed.</summary>
    None,

    /// <summary>The notification was dismissed by a user or consumer.</summary>
    User,

    /// <summary>The notification was dismissed because its expiration time was reached.</summary>
    Expired,

    /// <summary>The notification was replaced by a newer notification.</summary>
    Replaced,

    /// <summary>The source that created the notification was closed.</summary>
    SourceClosed,
}
