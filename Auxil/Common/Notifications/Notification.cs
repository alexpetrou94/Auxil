using System.Collections.Immutable;

namespace Auxil.Common.Notifications;

/// <summary>Immutable notification data and lifecycle snapshot.</summary>
[PublicAPI]
public sealed record Notification(string Title)
{
    /// <summary>The stable identifier assigned by a center or supplied by the publisher.</summary>
    public string? Id { get; init; }

    /// <summary>The monotonically increasing insertion sequence within a center.</summary>
    public long Sequence { get; internal init; }

    /// <summary>The logical source of the notification.</summary>
    public string? Source { get; init; }

    /// <summary>The source-defined category.</summary>
    public string? Category { get; init; }

    /// <summary>The notification severity.</summary>
    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Information;

    /// <summary>The optional notification body.</summary>
    public string? Message { get; init; }

    /// <summary>The time at which the notification was published.</summary>
    public DateTimeOffset CreatedAt { get; internal init; }

    /// <summary>The time at which the notification's state or content was last changed.</summary>
    public DateTimeOffset UpdatedAt { get; internal init; }

    /// <summary>The absolute time at which the notification may be expired.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Whether the notification has been marked as read.</summary>
    public bool IsRead { get; internal init; }

    /// <summary>The current lifecycle status.</summary>
    public NotificationStatus Status { get; internal init; } = NotificationStatus.Active;

    /// <summary>The dismissal reason, or <see cref="NotificationDismissReason.None"/> when not dismissed.</summary>
    public NotificationDismissReason DismissReason { get; internal init; } = NotificationDismissReason.None;

    /// <summary>The data-only actions associated with the notification.</summary>
    public ImmutableArray<NotificationAction> Actions { get; init; } = [];

    /// <summary>The source-defined tags associated with the notification.</summary>
    public ImmutableArray<string> Tags { get; init; } = [];

    /// <summary>The source-defined metadata associated with the notification.</summary>
    public ImmutableDictionary<string, string> Metadata { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>Whether the notification is currently active.</summary>
    public bool IsActive => Status == NotificationStatus.Active;

    /// <summary>Whether the notification is retained in history.</summary>
    public bool IsHistory => !IsActive;

    /// <summary>Returns whether the notification's expiration time has been reached.</summary>
    /// <param name="now">The time against which expiration is evaluated.</param>
    public bool IsExpired(DateTimeOffset now)
    {
        return ExpiresAt is { } expiresAt && expiresAt <= now;
    }
}
