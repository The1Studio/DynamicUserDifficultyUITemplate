namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;

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
            ILogger logger)
        {
            this.difficultyData = difficultyData;
            this.logger = logger;
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