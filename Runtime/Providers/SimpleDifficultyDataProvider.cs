namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using TheOne.Logging;

    /// <summary>
    /// Simple implementation of IDifficultyDataProvider that only handles difficulty storage.
    /// This breaks the circular dependency between the controller and service.
    /// </summary>
    public class SimpleDifficultyDataProvider : IDifficultyDataProvider
    {
        private readonly UITemplateDifficultyData difficultyData;
        private readonly ILogger logger;

        public SimpleDifficultyDataProvider(
            UITemplateDifficultyData difficultyData,
            ILoggerManager loggerManager = null)
        {
            this.difficultyData = difficultyData;
            this.logger = loggerManager?.GetLogger(this);
        }

        public float GetCurrentDifficulty()
        {
            return this.difficultyData.CurrentDifficulty;
        }

        public void SetCurrentDifficulty(float difficulty)
        {
            this.difficultyData.CurrentDifficulty = difficulty;
            this.logger?.Info($"[SimpleDifficultyDataProvider] Set difficulty to {difficulty:F1}");
        }
    }
}