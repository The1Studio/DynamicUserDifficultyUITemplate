namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    /// <summary>
    /// Constants specific to UITemplate integration.
    /// Only contains UI-specific mappings and display values.
    /// All core difficulty constants are in DifficultyConstants.
    /// </summary>
    public static class UITemplateIntegrationConstants
    {
        // UI difficulty level mapping for display/categorization
        // These map difficulty float values to UI categories
        public const float EASY_DIFFICULTY_VALUE = 2f;      // Difficulty 1-2 shown as "Easy"
        public const float NORMAL_DIFFICULTY_VALUE = 5f;    // Difficulty 3-5 shown as "Normal"
        public const float HARD_DIFFICULTY_VALUE = 8f;      // Difficulty 6-8 shown as "Hard"
        public const float VERY_HARD_DIFFICULTY_VALUE = 10f; // Difficulty 9-10 shown as "Very Hard"

        // UITemplate-specific fallback values when data is missing
        public const int DEFAULT_DAYS_AGO = 1;  // Default days for time decay when no session history
    }
}