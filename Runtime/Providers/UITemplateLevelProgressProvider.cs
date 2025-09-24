namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using global::UITemplate.Scripts.Enum;
    using TheOneStudio.UITemplate.UITemplate.Models;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides level progression data from UITemplate's level system.
    /// Tracks attempts, completion times, and difficulty patterns.
    /// </summary>
    [Preserve]
    public class UITemplateLevelProgressProvider : ILevelProgressProvider
    {
        private readonly UITemplateLevelDataController levelController;

        [Preserve]
        public UITemplateLevelProgressProvider(UITemplateLevelDataController levelController)
        {
            this.levelController = levelController ?? throw new ArgumentNullException(nameof(levelController));
        }

        /// <summary>
        /// Gets the current level the player is on.
        /// </summary>
        public int GetCurrentLevel()
        {
            return this.levelController?.CurrentLevel ?? 1;
        }

        /// <summary>
        /// Gets the number of attempts on the current level.
        /// </summary>
        public int GetAttemptsOnCurrentLevel()
        {
            try
            {
                var currentLevel = GetCurrentLevel();
                var levelData = this.levelController?.GetLevelData(currentLevel);

                if (levelData == null)
                {
                    return 0;
                }

                // Total attempts = wins + losses
                return levelData.WinCount + levelData.LoseCount;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateLevelProgressProvider] Error getting attempts on current level: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the static difficulty of the current level.
        /// </summary>
        public float GetCurrentLevelDifficulty()
        {
            try
            {
                var currentLevel = GetCurrentLevel();
                var levelData = this.levelController?.GetLevelData(currentLevel);

                if (levelData == null)
                {
                    return 3f; // Default medium difficulty
                }

                // Map LevelDifficulty enum to float value (0=Easy, 1=Normal, 2=Hard, 3=VeryHard)
                return levelData.StaticDifficulty switch
                {
                    LevelDifficulty.Easy => 2f,
                    LevelDifficulty.Normal => 5f,
                    LevelDifficulty.Hard => 8f,
                    LevelDifficulty.VeryHard => 10f,
                    _ => 3f
                };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateLevelProgressProvider] Error getting current level difficulty: {ex.Message}");
                return 3f;
            }
        }

        /// <summary>
        /// Gets the average time taken to complete levels.
        /// </summary>
        public float GetAverageCompletionTime()
        {
            try
            {
                var allLevels = this.levelController?.GetAllLevels();
                if (allLevels == null || allLevels.Count == 0)
                {
                    return 60f; // Default 60 seconds
                }

                // Get completed levels with valid win times
                var completedLevels = allLevels
                    .Where(l => l.LevelStatus == LevelData.Status.Passed && l.WinTime > 0)
                    .Select(l => (float)l.WinTime)
                    .ToList();

                if (completedLevels.Count == 0)
                {
                    return 60f;
                }

                return completedLevels.Average();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateLevelProgressProvider] Error getting average completion time: {ex.Message}");
                return 60f;
            }
        }

        /// <summary>
        /// Gets the completion rate (wins/total attempts) for recent levels.
        /// </summary>
        public float GetCompletionRate()
        {
            try
            {
                var allLevels = this.levelController?.GetAllLevels();
                if (allLevels == null || allLevels.Count == 0)
                {
                    return 0f;
                }

                var totalWins = allLevels.Sum(l => l.WinCount);
                var totalLosses = allLevels.Sum(l => l.LoseCount);
                var totalAttempts = totalWins + totalLosses;

                if (totalAttempts == 0)
                {
                    return 0f;
                }

                return (float)totalWins / totalAttempts;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateLevelProgressProvider] Error getting completion rate: {ex.Message}");
                return 0f;
            }
        }

    }
}