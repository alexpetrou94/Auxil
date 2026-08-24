namespace Auxil.Common.Notifications;

/// <summary>Defines the lifecycle state of a notification retained by a center.</summary>
[PublicAPI]
public enum NotificationStatus
{
    /// <summary>The notification is available to active consumers.</summary>
    Active,

    /// <summary>The notification was dismissed and is retained in history.</summary>
    Dismissed,

    /// <summary>The notification was archived and is retained in history.</summary>
    Archived
}
