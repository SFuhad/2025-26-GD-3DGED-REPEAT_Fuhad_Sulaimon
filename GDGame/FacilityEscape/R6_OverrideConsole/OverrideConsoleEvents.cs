namespace GDGame.FacilityEscape
{
    // two distinct custom event types, on purpose - R6's requirement is specifically about
    // publishing/subscribing more than one event type with different EventPriority presets,
    // not just one event doing everything.
    public sealed class OverrideActivatedEvent
    {
        public string ConsoleId { get; }

        public OverrideActivatedEvent(string consoleId)
        {
            ConsoleId = consoleId;
        }
    }

    public sealed class LockdownStateChangedEvent
    {
        public bool IsOverridden { get; }

        public LockdownStateChangedEvent(bool isOverridden)
        {
            IsOverridden = isOverridden;
        }
    }
}
