using System.Collections.Generic;

namespace GDGame.FacilityEscape
{
    // dead simple static tracker for which zones are done. Not persisted anywhere (resets
    // on game restart) - fine for a single playthrough. Same "static bag of state" style as
    // AppData, just mutable this time instead of readonly constants.
    public static class ZoneProgressState
    {
        public static readonly string[] AllZoneIds =
        {
            FacilityAppData.ZONE_ID_R1,
            FacilityAppData.ZONE_ID_R2,
            FacilityAppData.ZONE_ID_R3,
            FacilityAppData.ZONE_ID_R5,
            FacilityAppData.ZONE_ID_R6
        };

        private static readonly HashSet<string> _completed = new HashSet<string>();

        public static void MarkComplete(string zoneId)
        {
            _completed.Add(zoneId);
        }

        public static bool IsComplete(string zoneId)
        {
            return _completed.Contains(zoneId);
        }

        public static int RemainingCount
        {
            get { return AllZoneIds.Length - _completed.Count; }
        }

        // handy for testing - not called anywhere in normal play
        public static void ResetAll()
        {
            _completed.Clear();
        }
    }
}
