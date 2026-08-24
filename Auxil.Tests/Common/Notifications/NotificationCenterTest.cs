using System.Collections.Immutable;
using JetBrains.Annotations;
using Auxil.Common.Notifications;

namespace Auxil.Tests.Common.Notifications;

[TestSubject(typeof(NotificationCenter))]
public sealed class NotificationCenterTest
{
    private static readonly DateTimeOffset Start = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Publish_AssignsStableSequenceAndRaisesAddedEvent()
    {
        ManualTimeProvider clock = new(Start);
        NotificationCenter center = CreateCenter(clock);
        List<NotificationChangedEventArgs> changes = [];

        center.NotificationChanged += (_, args) => changes.Add(args);

        string firstId = center.Publish(CreateNotification("first", "first"));

        clock.Advance(TimeSpan.FromSeconds(1));

        string secondId = center.Publish(CreateNotification("second"));

        firstId.ShouldBe("first");

        secondId.ShouldNotBeNullOrWhiteSpace();
        secondId.ShouldNotBe(firstId);

        center.Count.ShouldBe(2);
        center.Active.Select(notification => notification.Id).ShouldBe(["first", secondId]);
        center.Active.Select(notification => notification.Sequence).ShouldBe([1L, 2L]);

        changes.Select(change => change.Kind).ShouldBe([
            NotificationChangeKind.Added,
            NotificationChangeKind.Added
        ]);

        changes[0].Notification.Id.ShouldBe(center.Active[0].Id);
        changes[0].Notification.Sequence.ShouldBe(center.Active[0].Sequence);
    }

    [Fact]
    public void Publish_RejectsDuplicateIdentifier()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start));
        center.Publish(CreateNotification("same", "same"));

        Should.Throw<ArgumentException>(() => center.Publish(CreateNotification("same again", "same")));
        center.Count.ShouldBe(1);
    }

    [Fact]
    public void Publish_ReturnsImmutableNotificationSnapshots()
    {
        ManualTimeProvider clock = new(Start);
        NotificationCenter center = CreateCenter(clock);

        string id = center.Publish(CreateNotification("snapshot", "snapshot") with
        {
            Actions = [new NotificationAction("open", "Open")],
            Tags = ["system"],
            Metadata = ImmutableDictionary<string, string>.Empty.Add("source-id", "42"),
        });

        Notification snapshot = center.Active[0];

        clock.Advance(TimeSpan.FromSeconds(1));
        center.Update(id, CreateNotification("updated", id));

        snapshot.Title.ShouldBe("snapshot");
        snapshot.Status.ShouldBe(NotificationStatus.Active);
        snapshot.Actions.Select(action => action.Id).ShouldBe(["open"]);
        snapshot.Tags.ShouldBe( ["system"]);
        snapshot.Metadata["source-id"].ShouldBe("42");
    }

    [Fact]
    public void Update_ReplacesContentButPreservesIdentityAndCreationTime()
    {
        ManualTimeProvider clock = new(Start);
        NotificationCenter center = CreateCenter(clock);
        string id = center.Publish(CreateNotification("before", "update"));

        clock.Advance(TimeSpan.FromMinutes(2));

        List<NotificationChangedEventArgs> changes = [];

        center.NotificationChanged += (_, args) => changes.Add(args);

        center.Update(id, new Notification("after")
        {
            Id = id,
            Source = "updated-source",
            Severity = NotificationSeverity.Warning,
            Message = "changed",
            ExpiresAt = Start.AddMinutes(7),
        }).ShouldBeTrue();

        center.TryGetValue(id, out Notification? record).ShouldBeTrue();

        record.ShouldNotBeNull();
        record.Id.ShouldBe(id);
        record.Sequence.ShouldBe(1);
        record.CreatedAt.ShouldBe(Start);
        record.UpdatedAt.ShouldBe(Start.AddMinutes(2));
        record.Source.ShouldBe("updated-source");
        record.Severity.ShouldBe(NotificationSeverity.Warning);
        record.Title.ShouldBe("after");
        record.Message.ShouldBe("changed");
        record.ExpiresAt.ShouldBe(Start.AddMinutes(7));

        changes.Count.ShouldBe(1);
        changes[0].Kind.ShouldBe(NotificationChangeKind.Updated);
    }

    [Fact]
    public void ReadStateChanges_KeepNotificationActiveAndAvoidDuplicateEvents()
    {
        ManualTimeProvider clock = new(Start);
        NotificationCenter center = CreateCenter(clock);
        string id = center.Publish(CreateNotification("readable", "readable"));
        List<NotificationChangedEventArgs> changes = [];
        center.NotificationChanged += (_, args) => changes.Add(args);

        center.MarkRead(id).ShouldBeTrue();
        center.MarkRead(id).ShouldBeFalse();
        center.MarkUnread(id).ShouldBeTrue();
        center.MarkUnread(id).ShouldBeFalse();

        center.Active.Length.ShouldBe(1);
        center.Active[0].IsRead.ShouldBeFalse();
        changes.Select(change => change.Kind).ShouldBe([
            NotificationChangeKind.ReadStateChanged,
            NotificationChangeKind.ReadStateChanged
        ]);
    }

    [Fact]
    public void Dismiss_MovesActiveRecordToHistoryWithReason()
    {
        ManualTimeProvider clock = new(Start);
        NotificationCenter center = CreateCenter(clock);
        string id = center.Publish(CreateNotification("dismiss", "dismiss"));
        clock.Advance(TimeSpan.FromSeconds(3));
        List<NotificationChangedEventArgs> changes = [];
        center.NotificationChanged += (_, args) => changes.Add(args);

        center.Dismiss(id, NotificationDismissReason.SourceClosed).ShouldBeTrue();
        center.Dismiss(id).ShouldBeFalse();

        center.Active.ShouldBeEmpty();
        center.History.Length.ShouldBe(1);
        Notification record = center.History[0];
        record.Status.ShouldBe(NotificationStatus.Dismissed);
        record.DismissReason.ShouldBe(NotificationDismissReason.SourceClosed);
        record.UpdatedAt.ShouldBe(Start.AddSeconds(3));
        changes.Select(change => change.Kind).ShouldBe([NotificationChangeKind.Dismissed]);
    }

    [Fact]
    public void Archive_MovesDismissedRecordToArchivedHistory()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start));
        string id = center.Publish(CreateNotification("archive", "archive"));

        center.Dismiss(id).ShouldBeTrue();
        center.Archive(id).ShouldBeTrue();
        center.Archive(id).ShouldBeFalse();

        center.History[0].Status.ShouldBe(NotificationStatus.Archived);
        center.History[0].DismissReason.ShouldBe(NotificationDismissReason.None);
    }

    [Fact]
    public void Remove_DeletesActiveAndHistoricalRecords()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start));
        string activeId = center.Publish(CreateNotification("active", "active"));
        string historyId = center.Publish(CreateNotification("history", "history"));
        center.Dismiss(historyId).ShouldBeTrue();

        center.Remove(activeId).ShouldBeTrue();
        center.Remove(historyId).ShouldBeTrue();
        center.Remove(historyId).ShouldBeFalse();

        center.Count.ShouldBe(0);
        center.All.ShouldBeEmpty();
        center.TryGetValue(activeId, out _).ShouldBeFalse();
        center.TryGetValue(historyId, out _).ShouldBeFalse();
    }

    [Fact]
    public void ClearHistory_RemovesOnlyHistoricalRecords()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start));
        string activeId = center.Publish(CreateNotification("active", "active"));
        string dismissedId = center.Publish(CreateNotification("dismissed", "dismissed"));
        string archivedId = center.Publish(CreateNotification("archived", "archived"));
        center.Dismiss(dismissedId).ShouldBeTrue();
        center.Archive(archivedId).ShouldBeTrue();

        center.ClearHistory().ShouldBe(2);
        center.Count.ShouldBe(1);
        center.Active.Select(notification => notification.Id).ShouldBe([activeId]);
        center.History.ShouldBeEmpty();
    }

    [Fact]
    public void HistoryLimit_RemovesOldestHistoricalRecords()
    {
        ManualTimeProvider clock = new(Start);
        NotificationCenter center = CreateCenter(clock, historyLimit: 1);
        List<NotificationChangedEventArgs> changes = [];
        center.NotificationChanged += (_, args) => changes.Add(args);
        string firstId = center.Publish(CreateNotification("first", "first"));
        string secondId = center.Publish(CreateNotification("second", "second"));

        center.Dismiss(firstId).ShouldBeTrue();
        center.Dismiss(secondId).ShouldBeTrue();

        center.History.Select(notification => notification.Id).ShouldBe([secondId]);
        center.TryGetValue(firstId, out _).ShouldBeFalse();
        changes.Any(change => change is
        {
            Kind: NotificationChangeKind.Removed,
            Notification.Id: var removedId,
        } && removedId == firstId).ShouldBeTrue();
    }

    [Fact]
    public void HistoryLimit_ZeroRetainsAllHistoricalRecords()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start), historyLimit: 0);
        string firstId = center.Publish(CreateNotification("first", "first"));
        string secondId = center.Publish(CreateNotification("second", "second"));
        string thirdId = center.Publish(CreateNotification("third", "third"));

        center.Dismiss(firstId).ShouldBeTrue();
        center.Dismiss(secondId).ShouldBeTrue();
        center.Dismiss(thirdId).ShouldBeTrue();

        center.HistoryLimit.ShouldBe(0u);
        center.History.Select(notification => notification.Id).ShouldBe([firstId, secondId, thirdId]);
        center.Count.ShouldBe(3);
    }

    [Fact]
    public void PruneExpired_DismissesOnlyDueActiveNotifications()
    {
        ManualTimeProvider clock = new(Start);
        NotificationCenter center = CreateCenter(clock);
        string expiringId = center.Publish(new Notification("expiring")
        {
            Id = "expiring",
            ExpiresAt = Start.AddMinutes(1),
        });
        string persistentId = center.Publish(CreateNotification("persistent", "persistent"));
        clock.Advance(TimeSpan.FromMinutes(1));

        center.PruneExpired().ShouldBe(1);
        center.PruneExpired().ShouldBe(0);
        center.Active.Select(notification => notification.Id).ShouldBe([persistentId]);
        center.History[0].Id.ShouldBe(expiringId);
        center.History[0].DismissReason.ShouldBe(NotificationDismissReason.Expired);
    }

    [Fact]
    public void InvokeAction_RaisesDataOnlyActionEvent()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start));
        string id = center.Publish(CreateNotification("actions", "actions") with
        {
            Actions = [new NotificationAction("open", "Open"), new NotificationAction("delete", "Delete", true)],
        });
        NotificationActionInvokedEventArgs? invoked = null;
        center.ActionInvoked += (_, args) => invoked = args;

        center.InvokeAction(id, "open").ShouldBeTrue();
        center.InvokeAction(id, "missing").ShouldBeFalse();

        invoked.ShouldNotBeNull();
        invoked.Notification.Id.ShouldBe(id);
        invoked.Action.Id.ShouldBe("open");
        invoked.Action.Label.ShouldBe("Open");
        center.Active.Length.ShouldBe(1);
    }

    [Fact]
    public void Validation_RejectsInvalidNotifications()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start));

        Should.Throw<ArgumentException>(() => center.Publish(new Notification(string.Empty)));
        Should.Throw<ArgumentException>(() => center.Publish(CreateNotification("duplicate tags") with
        {
            Tags = ["same", "same"],
        }));
        Should.Throw<ArgumentException>(() => center.Publish(CreateNotification("duplicate actions") with
        {
            Actions = [new NotificationAction("same", "One"), new NotificationAction("same", "Two")],
        }));
    }

    [Fact]
    public void Publish_IsSafeForConcurrentUniqueProducers()
    {
        NotificationCenter center = CreateCenter(new ManualTimeProvider(Start));

        Parallel.For(0, 100, index =>
        {
            center.Publish(CreateNotification($"notification-{index}", $"id-{index}"));
        });

        center.Count.ShouldBe(100);
        center.Active.Select(notification => notification.Sequence).Distinct().Count().ShouldBe(100);
        center.Active.Select(notification => notification.Id).Distinct(StringComparer.Ordinal).Count().ShouldBe(100);
    }

    private static NotificationCenter CreateCenter(ManualTimeProvider clock, uint historyLimit = 100)
    {
        return new NotificationCenter(new NotificationCenterOptions
        {
            HistoryLimit = historyLimit,
            TimeProvider = clock,
        });
    }

    private static Notification CreateNotification(string title, string? id = null)
    {
        return new Notification(title)
        {
            Id = id,
            Source = "tests",
            Category = "unit",
            Severity = NotificationSeverity.Information,
            Message = $"Message for {title}",
        };
    }

    private sealed class ManualTimeProvider(DateTimeOffset current) : TimeProvider
    {
        private DateTimeOffset Current { get; set; } = current;

        public override DateTimeOffset GetUtcNow()
        {
            return Current;
        }

        public void Advance(TimeSpan duration)
        {
            Current = Current.Add(duration);
        }
    }
}
