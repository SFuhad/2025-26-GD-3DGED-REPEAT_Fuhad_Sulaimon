using Microsoft.Xna.Framework;

namespace GDGame.FacilityEscape
{
    // same idea as the root AppData.cs - just a bag of constants so scene names/zone ids
    // aren't typo bait scattered across a dozen SceneBuilder files
    public static class FacilityAppData
    {
        #region Scene names
        public static readonly string HUB_SCENE_NAME = "FacilityHub";
        public static readonly string R1_SCENE_NAME = "FacilityR1_PhysicsLab";
        public static readonly string R2_SCENE_NAME = "FacilityR2_AudioCorridor";
        public static readonly string R3_SCENE_NAME = "FacilityR3_ObservationDeck";
        public static readonly string R5_SCENE_NAME = "FacilityR5_SecurityTerminal";
        public static readonly string R6_SCENE_NAME = "FacilityR6_OverrideConsole";
        #endregion

        #region Zone ids (used by ZoneProgressState / ZoneCompletedEvent - keep these in sync with the scene names above)
        public static readonly string ZONE_ID_R1 = "R1";
        public static readonly string ZONE_ID_R2 = "R2";
        public static readonly string ZONE_ID_R3 = "R3";
        public static readonly string ZONE_ID_R5 = "R5";
        public static readonly string ZONE_ID_R6 = "R6";
        #endregion

        #region Zone display names (for the hub progress overlay + door labels)
        public static readonly string R1_DISPLAY_NAME = "Physics Lab";
        public static readonly string R2_DISPLAY_NAME = "Audio Corridor";
        public static readonly string R3_DISPLAY_NAME = "Observation Deck";
        public static readonly string R5_DISPLAY_NAME = "Security Terminal";
        public static readonly string R6_DISPLAY_NAME = "Override Console";
        #endregion

        // where the player spawns when a scene is built - every zone just uses this,
        // room layout is built around it rather than the other way round
        public static readonly Vector3 DEFAULT_SPAWN_POSITION = new Vector3(0, 2, 0);

        // rooms are all the same footprint for now, easier to reason about than bespoke sizes per zone
        public static readonly Vector3 ROOM_SIZE = new Vector3(24, 6, 24);
    }
}
