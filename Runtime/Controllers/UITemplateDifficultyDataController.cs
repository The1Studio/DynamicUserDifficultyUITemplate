namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers
{
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine.Scripting;

    /// <summary>
    /// UITemplate data controller for Dynamic User Difficulty.
    /// Provides data access API for difficulty values.
    /// All business logic is handled by DifficultyAdapter.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyDataController : IUITemplateControllerData
    {
        private readonly UITemplateDifficultyData    difficultyData;

        [Preserve]
        public UITemplateDifficultyDataController(
            UITemplateDifficultyData difficultyData)
        {
            this.difficultyData    = difficultyData;
        }

        #region Data API

        /// <summary>
        /// Saves the current difficulty to local data storage.
        /// Called by DifficultyDataProvider when difficulty changes.
        /// </summary>
        /// <param name="difficulty">The difficulty value to save</param>
        public void UpdateDifficulty(float difficulty)
        {
            // Store previous difficulty before updating
            this.difficultyData.PreviousDifficulty = this.difficultyData.CurrentDifficulty;
            this.difficultyData.CurrentDifficulty = difficulty;
        }

        /// <summary>
        /// Loads the difficulty from local data storage.
        /// Called by DifficultyDataProvider during initialization.
        /// </summary>
        /// <returns>The stored difficulty value</returns>
        public float GetDifficulty()
        {
            return this.difficultyData.CurrentDifficulty;
        }

        /// <summary>
        /// Gets the previous difficulty value (before last adjustment).
        /// Used for session pattern analysis to track adjustment effectiveness.
        /// </summary>
        /// <returns>The previous difficulty value</returns>
        public float GetPreviousDifficulty()
        {
            return this.difficultyData.PreviousDifficulty;
        }

        #endregion
    }
}