namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    /// <summary>
    /// Constants specific to UITemplate integration.
    /// Only contains UI-specific mappings and display values.
    /// All core difficulty constants are in DifficultyConstants.
    /// </summary>
    public static class UITemplateIntegrationConstants
    {
        // UITemplate-specific fallback values when data is missing
        public const int DEFAULT_DAYS_AGO = 1;  // Default days for time decay when no session history
    }
}