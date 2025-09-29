namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.UserData;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Focused provider for difficulty data persistence only.
    /// Handles storing and retrieving the current difficulty value using UITemplate's data services.
    /// Follows UITemplate pattern of accessing data through IHandleUserDataServices instead of direct data access.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyDataProvider : IDifficultyDataProvider
    {
        private readonly IHandleUserDataServices handleUserDataServices;
        private readonly ILogger logger;
        private UITemplateDifficultyData cachedData;

        [Preserve]
        public UITemplateDifficultyDataProvider(IHandleUserDataServices handleUserDataServices, ILogger logger)
        {
            this.handleUserDataServices = handleUserDataServices ?? throw new ArgumentNullException(nameof(handleUserDataServices));
            this.logger = logger;

            // Load data synchronously on initialization
            this.LoadDataAsync().Forget();
        }

        /// <summary>
        /// Gets the current difficulty value from persistent storage.
        /// </summary>
        public float GetCurrentDifficulty()
        {
            // Return cached data if available, otherwise return default
            return this.cachedData?.CurrentDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY;
        }

        /// <summary>
        /// Sets and persists the current difficulty value.
        /// </summary>
        public void SetCurrentDifficulty(float newDifficulty)
        {
            // Ensure data is loaded
            if (this.cachedData == null)
            {
                this.LoadDataAsync().Forget();
            }

            if (this.cachedData != null)
            {
                this.cachedData.CurrentDifficulty = Mathf.Clamp(newDifficulty, DifficultyConstants.MIN_DIFFICULTY, DifficultyConstants.MAX_DIFFICULTY);

                // Save through the service
                this.handleUserDataServices.Save(this.cachedData);

                this.logger?.Info($"[UITemplateDifficultyDataProvider] Difficulty updated to {newDifficulty:F2}");
            }
        }

        private async UniTaskVoid LoadDataAsync()
        {
            try
            {
                this.cachedData = await this.handleUserDataServices.Load<UITemplateDifficultyData>();
                this.logger?.Info($"[UITemplateDifficultyDataProvider] Data loaded. Current difficulty: {this.cachedData?.CurrentDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY:F2}");
            }
            catch (Exception ex)
            {
                this.logger?.Error($"[UITemplateDifficultyDataProvider] Failed to load data: {ex.Message}");
                // Create default data if loading fails
                this.cachedData = new UITemplateDifficultyData();
            }
        }
    }
}