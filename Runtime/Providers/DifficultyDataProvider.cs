namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using UnityEngine;
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Implementation of IDifficultyDataProvider that uses UITemplateDifficultyDataController.
    /// This provider acts as a bridge between the difficulty service and UITemplate's data system.
    /// All data persistence is delegated to the controller which uses UITemplateDifficultyData.
    /// </summary>
    public class DifficultyDataProvider : IDifficultyDataProvider
    {
        private readonly UITemplateDifficultyDataController difficultyController;
        private readonly ILogger logger;

        public DifficultyDataProvider(
            UITemplateDifficultyDataController difficultyController,
            ILogger logger)
        {
            this.difficultyController = difficultyController ?? throw new ArgumentNullException(nameof(difficultyController));
            this.logger = logger;
        }

        public float GetCurrentDifficulty()
        {
            // Load from controller's data storage
            var difficulty = this.difficultyController.GetDifficulty();
            this.logger?.Debug($"[DifficultyDataProvider] Loaded difficulty: {difficulty:F1}");
            return difficulty;
        }

        public void SetCurrentDifficulty(float difficulty)
        {
            // Clamp difficulty to valid range
            var clampedDifficulty = Mathf.Clamp(difficulty, DifficultyConstants.MIN_DIFFICULTY, DifficultyConstants.MAX_DIFFICULTY);

            // Save through controller's data API
            this.difficultyController.UpdateDifficulty(clampedDifficulty);

            this.logger?.Info($"[DifficultyDataProvider] Saved difficulty: {clampedDifficulty:F1}");
        }
    }
}