using System.Collections.Immutable;

namespace Auxil.Common.Notifications;

/// <summary>
/// Stores immutable notification data and coordinates its lifecycle without making presentation decisions.
/// </summary>
[PublicAPI]
public sealed class NotificationCenter
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, Notification> _notifications = new(StringComparer.Ordinal);
    private readonly List<string> _order = [];
    private readonly TimeProvider _timeProvider;
    private long _nextSequence;

    /// <summary>Creates an empty notification center.</summary>
    /// <param name="options">Optional history and clock configuration.</param>
    public NotificationCenter(NotificationCenterOptions? options = null)
    {
        options ??= new NotificationCenterOptions();

        ArgumentNullException.ThrowIfNull(options.TimeProvider);

        HistoryLimit = options.HistoryLimit;
        _timeProvider = options.TimeProvider;
    }

    /// <summary>Raised after a notification is added or its lifecycle/content changes.</summary>
    public event EventHandler<NotificationChangedEventArgs>? NotificationChanged;

    /// <summary>Raised when a consumer selects a data-only notification action.</summary>
    public event EventHandler<NotificationActionInvokedEventArgs>? ActionInvoked;

    /// <summary>The configured number of historical records retained by this center.</summary>
    public uint HistoryLimit { get; }

    /// <summary>The number of records currently retained, including active and historical records.</summary>
    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _notifications.Count;
            }
        }
    }

    /// <summary>Gets an immutable snapshot of active notifications in insertion order.</summary>
    public ImmutableArray<Notification> Active => GetSnapshot(static notification => notification.Status == NotificationStatus.Active);

    /// <summary>Gets an immutable snapshot of dismissed and archived notifications in insertion order.</summary>
    public ImmutableArray<Notification> History => GetSnapshot(static notification => notification.Status != NotificationStatus.Active);

    /// <summary>Gets an immutable snapshot of all retained notifications in insertion order.</summary>
    public ImmutableArray<Notification> All => GetSnapshot(static _ => true);

    /// <summary>Publishes a new active notification and returns its stable identifier.</summary>
    /// <param name="notification">The notification data to publish.</param>
    /// <exception cref="ArgumentException">Thrown when the notification is invalid or its identifier is already retained.</exception>
    public string Publish(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ValidateNotification(notification);

        NotificationChangedEventArgs change;
        string id;

        lock (_gate)
        {
            id = notification.Id ?? CreateIdLocked();

            if (_notifications.ContainsKey(id))
            {
                throw new ArgumentException($"A notification with id '{id}' is already retained.", nameof(notification));
            }

            DateTimeOffset now = _timeProvider.GetUtcNow();
            Notification added = Normalize(notification) with
            {
                Id = id,
                Sequence = ++_nextSequence,
                CreatedAt = now,
                UpdatedAt = now,
                IsRead = false,
                Status = NotificationStatus.Active,
                DismissReason = NotificationDismissReason.None,
            };
            _notifications.Add(id, added);
            _order.Add(id);
            change = new NotificationChangedEventArgs(NotificationChangeKind.Added, added);
        }

        NotificationChanged?.Invoke(this, change);
        return id;
    }

    /// <summary>Replaces the content of an existing notification without changing its identity or lifecycle status.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="notification">The complete replacement data. Its optional identifier must match <paramref name="id"/> when supplied.</param>
    /// <returns><c>true</c> when the notification existed and was updated; otherwise <c>false</c>.</returns>
    public bool Update(string id, Notification notification)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(notification);
        ValidateNotification(notification);

        if (notification.Id is not null && !string.Equals(notification.Id, id, StringComparison.Ordinal))
        {
            throw new ArgumentException("An update notification identifier must match the notification identifier.", nameof(notification));
        }

        NotificationChangedEventArgs change;

        lock (_gate)
        {
            if (!_notifications.TryGetValue(id, out Notification? stored))
            {
                return false;
            }

            DateTimeOffset now = _timeProvider.GetUtcNow();
            Notification updated = Normalize(notification) with
            {
                Id = id,
                Sequence = stored.Sequence,
                CreatedAt = stored.CreatedAt,
                UpdatedAt = now,
                IsRead = stored.IsRead,
                Status = stored.Status,
                DismissReason = stored.DismissReason,
            };
            _notifications[id] = updated;
            change = new NotificationChangedEventArgs(NotificationChangeKind.Updated, updated);
        }

        NotificationChanged?.Invoke(this, change);
        return true;
    }

    /// <summary>Marks an existing notification as read.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <returns><c>true</c> when the read state changed; otherwise <c>false</c>.</returns>
    public bool MarkRead(string id)
    {
        return SetReadState(id, true);
    }

    /// <summary>Marks an existing notification as unread.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <returns><c>true</c> when the read state changed; otherwise <c>false</c>.</returns>
    public bool MarkUnread(string id)
    {
        return SetReadState(id, false);
    }

    /// <summary>Dismisses an active notification and retains it in history.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="reason">The reason for dismissal.</param>
    /// <returns><c>true</c> when the notification was dismissed; otherwise <c>false</c>.</returns>
    public bool Dismiss(string id, NotificationDismissReason reason = NotificationDismissReason.User)
    {
        ValidateId(id);

        List<NotificationChangedEventArgs> changes = [];

        lock (_gate)
        {
            if (!_notifications.TryGetValue(id, out Notification? stored) || stored.Status != NotificationStatus.Active)
            {
                return false;
            }

            Notification dismissed = stored with
            {
                Status = NotificationStatus.Dismissed,
                DismissReason = reason,
                UpdatedAt = _timeProvider.GetUtcNow(),
            };
            _notifications[id] = dismissed;
            changes.Add(new NotificationChangedEventArgs(NotificationChangeKind.Dismissed, dismissed));
            TrimHistoryLocked(changes);
        }

        RaiseChanges(changes);
        return true;
    }

    /// <summary>Archives an active or dismissed notification and retains it in history.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <returns><c>true</c> when the notification was archived; otherwise <c>false</c>.</returns>
    public bool Archive(string id)
    {
        ValidateId(id);

        List<NotificationChangedEventArgs> changes = [];

        lock (_gate)
        {
            if (!_notifications.TryGetValue(id, out Notification? stored) || stored.Status == NotificationStatus.Archived)
            {
                return false;
            }

            Notification archived = stored with
            {
                Status = NotificationStatus.Archived,
                DismissReason = NotificationDismissReason.None,
                UpdatedAt = _timeProvider.GetUtcNow(),
            };
            _notifications[id] = archived;
            changes.Add(new NotificationChangedEventArgs(NotificationChangeKind.Archived, archived));
            TrimHistoryLocked(changes);
        }

        RaiseChanges(changes);
        return true;
    }

    /// <summary>Removes a notification from active records or history.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <returns><c>true</c> when the notification was removed; otherwise <c>false</c>.</returns>
    public bool Remove(string id)
    {
        ValidateId(id);

        NotificationChangedEventArgs change;

        lock (_gate)
        {
            if (!_notifications.Remove(id, out Notification? removed))
            {
                return false;
            }

            _order.Remove(id);
            change = new NotificationChangedEventArgs(NotificationChangeKind.Removed, removed);
        }

        NotificationChanged?.Invoke(this, change);
        return true;
    }

    /// <summary>Removes all dismissed and archived records while leaving active notifications untouched.</summary>
    /// <returns>The number of historical records removed.</returns>
    public int ClearHistory()
    {
        List<NotificationChangedEventArgs> changes = [];

        lock (_gate)
        {
            foreach (string id in _order.ToArray())
            {
                if (!_notifications.TryGetValue(id, out Notification? stored) || stored.Status == NotificationStatus.Active)
                {
                    continue;
                }

                _notifications.Remove(id);
                _order.Remove(id);
                changes.Add(new NotificationChangedEventArgs(NotificationChangeKind.Removed, stored));
            }
        }

        RaiseChanges(changes);
        return changes.Count;
    }

    /// <summary>Dismisses active notifications whose expiration time has been reached.</summary>
    /// <param name="now">The time used for expiration checks, or the configured clock time when omitted.</param>
    /// <returns>The number of notifications dismissed as expired.</returns>
    public int PruneExpired(DateTimeOffset? now = null)
    {
        DateTimeOffset expirationTime = now ?? _timeProvider.GetUtcNow();
        List<NotificationChangedEventArgs> changes = [];

        lock (_gate)
        {
            foreach (string id in _order.ToArray())
            {
                if (!_notifications.TryGetValue(id, out Notification? stored) ||
                    stored.Status != NotificationStatus.Active ||
                    stored.ExpiresAt is not { } expiresAt ||
                    expiresAt > expirationTime)
                {
                    continue;
                }

                Notification expired = stored with
                {
                    Status = NotificationStatus.Dismissed,
                    DismissReason = NotificationDismissReason.Expired,
                    UpdatedAt = expirationTime,
                };
                _notifications[id] = expired;
                changes.Add(new NotificationChangedEventArgs(NotificationChangeKind.Dismissed, expired));
            }

            TrimHistoryLocked(changes);
        }

        RaiseChanges(changes);
        return changes.Count(static change => change.Kind == NotificationChangeKind.Dismissed);
    }

    /// <summary>Raises an action event when the requested action exists on a retained notification.</summary>
    /// <param name="notificationId">The notification identifier.</param>
    /// <param name="actionId">The action identifier.</param>
    /// <returns><c>true</c> when the action was found and the event was raised; otherwise <c>false</c>.</returns>
    public bool InvokeAction(string notificationId, string actionId)
    {
        ValidateId(notificationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);

        NotificationActionInvokedEventArgs eventArgs;

        lock (_gate)
        {
            if (!_notifications.TryGetValue(notificationId, out Notification? notification))
            {
                return false;
            }

            NotificationAction? action = notification.Actions.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, actionId, StringComparison.Ordinal));
            if (action is null)
            {
                return false;
            }

            eventArgs = new NotificationActionInvokedEventArgs(notification, action);
        }

        ActionInvoked?.Invoke(this, eventArgs);
        return true;
    }

    /// <summary>Tries to retrieve an immutable snapshot by identifier.</summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="notification">The notification when found; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> when the notification exists; otherwise <c>false</c>.</returns>
    public bool TryGetValue(string id, out Notification? notification)
    {
        ValidateId(id);

        lock (_gate)
        {
            if (_notifications.TryGetValue(id, out Notification? stored))
            {
                notification = stored;
                return true;
            }
        }

        notification = null;
        return false;
    }

    private bool SetReadState(string id, bool isRead)
    {
        ValidateId(id);

        NotificationChangedEventArgs change;

        lock (_gate)
        {
            if (!_notifications.TryGetValue(id, out Notification? stored) || stored.IsRead == isRead)
            {
                return false;
            }

            Notification updated = stored with
            {
                IsRead = isRead,
                UpdatedAt = _timeProvider.GetUtcNow(),
            };
            _notifications[id] = updated;
            change = new NotificationChangedEventArgs(NotificationChangeKind.ReadStateChanged, updated);
        }

        NotificationChanged?.Invoke(this, change);
        return true;
    }

    private ImmutableArray<Notification> GetSnapshot(Func<Notification, bool> predicate)
    {
        lock (_gate)
        {
            ImmutableArray<Notification>.Builder snapshots = ImmutableArray.CreateBuilder<Notification>();

            foreach (string id in _order)
            {
                Notification notification = _notifications[id];
                if (predicate(notification))
                {
                    snapshots.Add(notification);
                }
            }

            return snapshots.ToImmutable();
        }
    }

    private string CreateIdLocked()
    {
        string id;
        do
        {
            id = Guid.NewGuid().ToString("N");
        }
        while (_notifications.ContainsKey(id));

        return id;
    }

    private void TrimHistoryLocked(List<NotificationChangedEventArgs> changes)
    {
        if (HistoryLimit == 0)
        {
            return;
        }

        uint historyCount = (uint)_notifications.Values.Count(static notification => notification.Status != NotificationStatus.Active);

        while (historyCount > HistoryLimit)
        {
            string? oldestHistoryId = null;
            foreach (string id in _order)
            {
                if (_notifications[id].Status == NotificationStatus.Active)
                {
                    continue;
                }

                oldestHistoryId = id;
                break;
            }

            if (oldestHistoryId is null || !_notifications.Remove(oldestHistoryId, out Notification? removed))
            {
                break;
            }

            _order.Remove(oldestHistoryId);
            changes.Add(new NotificationChangedEventArgs(NotificationChangeKind.Removed, removed));
            historyCount--;
        }
    }

    private static void ValidateId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
    }

    private static void ValidateNotification(Notification notification)
    {
        if (string.IsNullOrWhiteSpace(notification.Title))
        {
            throw new ArgumentException("A notification title is required.", nameof(notification));
        }

        if (notification.Id is not null)
        {
            ValidateId(notification.Id);
        }

        HashSet<string> actionIds = new(StringComparer.Ordinal);
        foreach (NotificationAction action in notification.Actions)
        {
            ArgumentNullException.ThrowIfNull(action);
            if (!actionIds.Add(action.Id))
            {
                throw new ArgumentException($"Duplicate notification action id '{action.Id}'.", nameof(notification));
            }
        }

        HashSet<string> tags = new(StringComparer.Ordinal);
        foreach (string tag in notification.Tags)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            if (!tags.Add(tag))
            {
                throw new ArgumentException($"Duplicate notification tag '{tag}'.", nameof(notification));
            }
        }

        foreach ((string key, string value) in notification.Metadata)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);
        }
    }

    private static Notification Normalize(Notification notification)
    {
        return notification with
        {
            Actions = CopyActions(notification.Actions),
            Tags = CopyTags(notification.Tags),
            Metadata = CopyMetadata(notification.Metadata),
        };
    }

    private static ImmutableArray<NotificationAction> CopyActions(ImmutableArray<NotificationAction> actions)
    {
        return actions.IsDefaultOrEmpty ? [] : [.. actions];
    }

    private static ImmutableArray<string> CopyTags(ImmutableArray<string> tags)
    {
        return tags.IsDefaultOrEmpty ? [] : [.. tags];
    }

    private static ImmutableDictionary<string, string> CopyMetadata(ImmutableDictionary<string, string> metadata)
    {
        if (metadata.IsEmpty)
        {
            return ImmutableDictionary<string, string>.Empty;
        }

        ImmutableDictionary<string, string>.Builder copy = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in metadata)
        {
            copy.Add(key, value);
        }

        return copy.ToImmutable();
    }

    private void RaiseChanges(List<NotificationChangedEventArgs> changes)
    {
        foreach (NotificationChangedEventArgs change in changes)
        {
            NotificationChanged?.Invoke(this, change);
        }
    }
}
