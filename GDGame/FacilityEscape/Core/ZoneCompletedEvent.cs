namespace GDGame.FacilityEscape
{
    // published on the shared EventBus whenever a zone's win condition is satisfied.
    // this is the "Subject" side of the Observer pattern for this project - Hub-side
    // listeners (ZoneProgressController, AudioResponder) are the Observers, and they
    // don't care which zone published it, just the id.
    public sealed class ZoneCompletedEvent
    {
        public string ZoneId { get; }

        public ZoneCompletedEvent(string zoneId)
        {
            ZoneId = zoneId;
        }
    }
}
