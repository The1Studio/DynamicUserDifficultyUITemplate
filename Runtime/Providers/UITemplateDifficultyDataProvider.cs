namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Focused provider for difficulty data persistence only.
    /// Handles storing and retrieving the current difficulty value.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyDataProvider : IDifficultyDataProvider
    {
        private readonly UITemplateDifficultyData difficultyData;

        [Preserve]
        public UITemplateDifficultyDataProvider(UITemplateDifficultyData difficultyData)
        {
            this.difficultyData = difficultyData ?? throw new ArgumentNullException(nameof(difficultyData));
        }

        /// <summary>
        /// Gets the current difficulty value from persistent storage.
        /// </summary>
        public float GetCurrentDifficulty()
        {
            return this.difficultyData?.CurrentDifficulty ?? 3f; // Default difficulty if null
        }

        /// <summary>
        /// Sets and persists the current difficulty value.
        /// </summary>
        public void SetCurrentDifficulty(float newDifficulty)
        {
            if (this.difficultyData != null)
            {
                this.difficultyData.CurrentDifficulty = Mathf.Clamp(newDifficulty, 1f, 10f);
                Debug.Log($"[UITemplateDifficultyDataProvider] Difficulty updated to {newDifficulty:F2}");
            }
        }
    }
}