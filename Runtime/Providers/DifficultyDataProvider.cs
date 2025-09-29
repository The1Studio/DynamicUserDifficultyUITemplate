namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.UserData;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using UnityEngine;

    /// <summary>
    /// Simple implementation of IDifficultyDataProvider that only handles difficulty storage.
    /// This breaks the circular dependency between the controller and service.
    /// Uses IHandleUserDataServices for proper data management following UITemplate patterns.
    /// </summary>
    public class DifficultyDataProvider : IDifficultyDataProvider
    {
        private readonly IHandleUserDataServices handleUserDataServices;
        private readonly ILogger logger;
        private UITemplateDifficultyData cachedData;

        public DifficultyDataProvider(
            IHandleUserDataServices handleUserDataServices,
            ILogger logger)
        {
            this.handleUserDataServices = handleUserDataServices ?? throw new ArgumentNullException(nameof(handleUserDataServices));
            this.logger = logger;

            // Load data on initialization
            this.LoadDataAsync().Forget();
        }

        public float GetCurrentDifficulty()
        {
            return this.cachedData?.CurrentDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY;
        }

        public void SetCurrentDifficulty(float difficulty)
        {
            // Ensure data is loaded
            if (this.cachedData == null)
            {
                this.LoadDataAsync().Forget();
            }

            if (this.cachedData != null)
            {
                this.cachedData.CurrentDifficulty = Mathf.Clamp(difficulty, DifficultyConstants.MIN_DIFFICULTY, DifficultyConstants.MAX_DIFFICULTY);

                // Save through the service
                this.handleUserDataServices.Save(this.cachedData);

                this.logger?.Info($"[SimpleDifficultyDataProvider] Set difficulty to {difficulty:F1}");
            }
        }

        private async UniTaskVoid LoadDataAsync()
        {
            try
            {
                this.cachedData = await this.handleUserDataServices.Load<UITemplateDifficultyData>();
                this.logger?.Info($"[SimpleDifficultyDataProvider] Data loaded. Current difficulty: {this.cachedData?.CurrentDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY:F1}");
            }
            catch (Exception ex)
            {
                this.logger?.Error($"[SimpleDifficultyDataProvider] Failed to load data: {ex.Message}");
                // Create default data if loading fails
                this.cachedData = new UITemplateDifficultyData();
            }
        }
    }
}