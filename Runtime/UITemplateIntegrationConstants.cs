namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    /// <summary>
    /// Constants used across the UITemplate integration module.
    /// Centralizes all hardcoded values for easier maintenance and configuration.
    /// </summary>
    public static class UITemplateIntegrationConstants
    {
        // Difficulty range constants
        public const float DEFAULT_DIFFICULTY = 3f;
        public const float MIN_DIFFICULTY = 1f;
        public const float MAX_DIFFICULTY = 10f;

        // Time-based constants
        public const float DEFAULT_COMPLETION_TIME_SECONDS = 60f;
        public const float DEFAULT_SESSION_DURATION_SECONDS = 60f;
        public const int DEFAULT_DAYS_AGO = 1;
        public const float SHORT_SESSION_THRESHOLD_SECONDS = 10f;
        public const float NORMAL_SESSION_THRESHOLD_SECONDS = 1800f; // 30 minutes

        // Level difficulty mapping constants
        public const float EASY_DIFFICULTY_VALUE = 2f;
        public const float NORMAL_DIFFICULTY_VALUE = 5f;
        public const float HARD_DIFFICULTY_VALUE = 8f;
        public const float VERY_HARD_DIFFICULTY_VALUE = 10f;

        // Session history constants
        public const int MAX_SESSIONS_TO_CHECK = 10;

        // Game parameter mapping ranges
        // Screw Speed
        public const float SCREW_SPEED_SLOW_MIN = 0.8f;
        public const float SCREW_SPEED_SLOW_MAX = 1.0f;
        public const float SCREW_SPEED_NORMAL_MIN = 1.0f;
        public const float SCREW_SPEED_NORMAL_MAX = 1.2f;
        public const float SCREW_SPEED_FAST_MIN = 1.2f;
        public const float SCREW_SPEED_FAST_MAX = 1.5f;

        // Time Limits
        public const float TIME_LIMIT_EASY_MAX = 180f;
        public const float TIME_LIMIT_EASY_MIN = 120f;
        public const float TIME_LIMIT_NORMAL_MAX = 120f;
        public const float TIME_LIMIT_NORMAL_MIN = 90f;
        public const float TIME_LIMIT_HARD_MAX = 90f;
        public const float TIME_LIMIT_HARD_MIN = 60f;

        // Target Score ranges
        public const int TARGET_SCORE_EASY_MIN = 100;
        public const int TARGET_SCORE_EASY_MAX = 300;
        public const int TARGET_SCORE_NORMAL_MIN = 300;
        public const int TARGET_SCORE_NORMAL_MAX = 600;
        public const int TARGET_SCORE_HARD_MIN = 600;
        public const int TARGET_SCORE_HARD_MAX = 1000;

        // Hint Delay ranges
        public const float HINT_DELAY_EASY_MIN = 5f;
        public const float HINT_DELAY_EASY_MAX = 10f;
        public const float HINT_DELAY_NORMAL_MIN = 10f;
        public const float HINT_DELAY_NORMAL_MAX = 20f;
        public const float HINT_DELAY_HARD_MIN = 20f;
        public const float HINT_DELAY_HARD_MAX = 30f;

        // Booster Effectiveness ranges
        public const float BOOSTER_EFFECT_EASY_MAX = 1.5f;
        public const float BOOSTER_EFFECT_EASY_MIN = 1.2f;
        public const float BOOSTER_EFFECT_NORMAL_MAX = 1.2f;
        public const float BOOSTER_EFFECT_NORMAL_MIN = 1.0f;
        public const float BOOSTER_EFFECT_HARD_MAX = 1.0f;
        public const float BOOSTER_EFFECT_HARD_MIN = 0.8f;

        // Difficulty thresholds for parameter mapping
        public const float EASY_DIFFICULTY_THRESHOLD = 3f;
        public const float NORMAL_DIFFICULTY_THRESHOLD = 7f;

        // Calculation factors
        public const float EASY_SCREW_SPEED_FACTOR = 0.1f;
        public const float NORMAL_SCREW_SPEED_FACTOR = 0.067f;
        public const float HARD_SCREW_SPEED_FACTOR = 0.15f;

        public const float EASY_TIME_REDUCTION_FACTOR = 30f;
        public const float NORMAL_TIME_REDUCTION_FACTOR = 10f;
        public const float HARD_TIME_REDUCTION_FACTOR = 15f;

        public const float EASY_SCORE_INCREMENT = 100f;
        public const float NORMAL_SCORE_INCREMENT = 100f;
        public const float HARD_SCORE_INCREMENT = 200f;

        public const float EASY_HINT_INCREMENT = 2.5f;
        public const float NORMAL_HINT_INCREMENT = 3.33f;
        public const float HARD_HINT_INCREMENT = 5f;

        public const float EASY_BOOSTER_DECREMENT = 0.15f;
        public const float NORMAL_BOOSTER_DECREMENT = 0.067f;
        public const float HARD_BOOSTER_DECREMENT = 0.1f;

        // Piece distance validation
        public const float MIN_PIECE_DISTANCE = 0.5f;
    }
}