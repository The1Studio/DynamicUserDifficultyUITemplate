namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Focused provider for difficulty data persistence only.
    /// Handles storing and retrieving the current difficulty value.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyDataProvider : IDifficultyDataProvider
    {
        private readonly UITemplateDifficultyData difficultyData;
        private readonly ILogger logger;

        [Preserve]
        public UITemplateDifficultyDataProvider(UITemplateDifficultyData difficultyData, ILogger logger)
        {
            this.difficultyData = difficultyData ?? throw new ArgumentNullException(nameof(difficultyData));
            this.logger = logger;
        }

        /// <summary>
        /// Gets the current difficulty value from persistent storage.
        /// </summary>
        public float GetCurrentDifficulty()
        {
            return this.difficultyData?.CurrentDifficulty ?? UITemplateIntegrationConstants.DEFAULT_DIFFICULTY;
        }

        /// <summary>
        /// Sets and persists the current difficulty value.
        /// </summary>
        public void SetCurrentDifficulty(float newDifficulty)
        {
            if (this.difficultyData != null)
            {
                this.difficultyData.CurrentDifficulty = Mathf.Clamp(newDifficulty, UITemplateIntegrationConstants.MIN_DIFFICULTY, UITemplateIntegrationConstants.MAX_DIFFICULTY);
                this.logger?.Info($"[UITemplateDifficultyDataProvider] Difficulty updated to {newDifficulty:F2}");
            }
        }
    }
}