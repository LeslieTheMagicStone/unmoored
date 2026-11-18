namespace CGJ2026
{
    /// <summary>
    /// Event ids for <see cref="EventBus"/>. Payloads: PlayerAnchored, PlayerReleased and
    /// PlayerFell carry the <see cref="PlayerController"/>; LivesChanged carries the remaining lives (int);
    /// CheckpointReached carries the <see cref="Checkpoint"/>; the rest carry null.
    /// </summary>
    public static class E
    {
        public const string ScoreChanged      = "score_changed";
        public const string LevelComplete     = "level_complete";   // one level beaten
        public const string GameClear         = "game_clear";       // all levels beaten
        public const string PlayerAnchored    = "player_anchored";
        public const string PlayerReleased    = "player_released";
        public const string PlayerFell        = "player_fell";
        public const string LivesChanged      = "lives_changed";
        public const string GameOver          = "game_over";        // lives exhausted
        public const string CheckpointReached = "checkpoint_reached";
        public const string PlayerRespawned   = "player_respawned";
    }
}
