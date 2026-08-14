namespace GDGame.FacilityEscape
{
    // fires when the player activates R2's console - kept separate from ZoneCompletedEvent
    // since it's a local "something happened in this zone" event, not the cross-zone one.
    public sealed class ConsoleActivatedEvent
    {
    }
}
