namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using UnityEngine;
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Simple implementation of IDifficultyDataProvider that delegates to UITemplateDifficultyDataController.
    /// This avoids direct data access and follows the UITemplate controller pattern.
    /// The controller handles all data persistence through the existing UITemplate system.
    /// </summary>
    public class DifficultyDataProvider : IDifficultyDataProvider
    {
        private readonly UITemplateDifficultyDataController difficultyController;
        private readonly IDynamicDifficultyService difficultyService;
        private readonly ILogger logger;

        public DifficultyDataProvider(
            UITemplateDifficultyDataController difficultyController,
            IDynamicDifficultyService difficultyService,
            ILogger logger)
        {
            this.difficultyController = difficultyController ?? throw new ArgumentNullException(nameof(difficultyController));
            this.difficultyService = difficultyService ?? throw new ArgumentNullException(nameof(difficultyService));
            this.logger = logger;
        }

        public float GetCurrentDifficulty()
        {
            // Use the controller's CurrentDifficulty property which gets it from the service
            return this.difficultyController.CurrentDifficulty;
        }

        public void SetCurrentDifficulty(float difficulty)
        {
            // Apply difficulty through the service (which the controller also uses)
            var clampedDifficulty = Mathf.Clamp(difficulty, DifficultyConstants.MIN_DIFFICULTY, DifficultyConstants.MAX_DIFFICULTY);
            
            var result = new DifficultyResult
            {
                NewDifficulty = clampedDifficulty,
                PreviousDifficulty = this.difficultyController.CurrentDifficulty
            };
            
            this.difficultyService.ApplyDifficulty(result);
            
            this.logger?.Info($"[DifficultyDataProvider] Set difficulty to {clampedDifficulty:F1} via controller");
        }
    }
}