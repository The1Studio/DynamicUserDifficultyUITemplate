namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    /// <summary>
    /// Constants used across the UITemplate integration module.
    /// Only contains generic constants that apply to any game using UITemplate.
    /// Game-specific constants should be defined in the game's own codebase.
    /// </summary>
    public static class UITemplateIntegrationConstants
    {
        // Difficulty range constants (generic for all games)
        public const float DEFAULT_DIFFICULTY = 5f;
        public const float MIN_DIFFICULTY = 1f;
        public const float MAX_DIFFICULTY = 10f;

        // Difficulty change threshold
        public const float DIFFICULTY_CHANGE_THRESHOLD = 0.01f;

        // Time-based constants (generic defaults)
        public const float DEFAULT_COMPLETION_TIME_SECONDS = 60f;
        public const float DEFAULT_SESSION_DURATION_SECONDS = 60f;
        public const int DEFAULT_DAYS_AGO = 1;
        public const float SHORT_SESSION_THRESHOLD_SECONDS = 10f;
        public const float NORMAL_SESSION_THRESHOLD_SECONDS = 1800f; // 30 minutes

        // Generic difficulty level mapping (for UI display or categorization)
        public const float EASY_DIFFICULTY_VALUE = 2f;
        public const float NORMAL_DIFFICULTY_VALUE = 5f;
        public const float HARD_DIFFICULTY_VALUE = 8f;
        public const float VERY_HARD_DIFFICULTY_VALUE = 10f;

        // Session history constants
        public const int MAX_SESSIONS_TO_CHECK = 10;
    }
}