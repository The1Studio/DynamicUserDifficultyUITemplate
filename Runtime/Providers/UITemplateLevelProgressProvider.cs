namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using System.Linq;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides level progression data from UITemplate's level system.
    /// Tracks attempts, completion times, and difficulty patterns.
    /// </summary>
    [Preserve]
    public class UITemplateLevelProgressProvider : ILevelProgressProvider
    {
        private readonly UITemplateLevelDataController levelController;
        private readonly ILogger logger;

        [Preserve]
        public UITemplateLevelProgressProvider(UITemplateLevelDataController levelController, ILogger logger)
        {
            this.levelController = levelController ?? throw new ArgumentNullException(nameof(levelController));
            this.logger = logger;
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
                var currentLevel = this.GetCurrentLevel();
                var currentMode  = this.levelController?.CurrentMode ?? UITemplateUserLevelData.ClassicMode;
                var levelData    = this.levelController?.GetLevelData(currentLevel, currentMode);

                if (levelData == null)
                {
                    return 0;
                }

                // Total attempts = wins + losses
                return levelData.WinCount + levelData.LoseCount;
            }
            catch (Exception ex)
            {
                this.logger?.Warning($"[UITemplateLevelProgressProvider] Error getting attempts on current level: {ex.Message}");
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
                var levelData   = this.levelController.GetCurrentLevelData;

                if (levelData == null)
                {
                    return DifficultyConstants.DEFAULT_DIFFICULTY;
                }

                return levelData.DynamicDifficulty;
            }
            catch (Exception ex)
            {
                this.logger?.Warning($"[UITemplateLevelProgressProvider] Error getting current level difficulty: {ex.Message}");
                return DifficultyConstants.DEFAULT_DIFFICULTY;
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
                    return DifficultyConstants.DEFAULT_COMPLETION_TIME_SECONDS;
                }

                // Get completed levels with valid win times
                var completedLevels = allLevels
                    .Where(l => l.LevelStatus == LevelData.Status.Passed && l.WinTime > 0)
                    .Select(l => (float)l.WinTime)
                    .ToList();

                if (completedLevels.Count == 0)
                {
                    return DifficultyConstants.DEFAULT_COMPLETION_TIME_SECONDS;
                }

                return completedLevels.Average();
            }
            catch (Exception ex)
            {
                this.logger?.Warning($"[UITemplateLevelProgressProvider] Error getting average completion time: {ex.Message}");
                return DifficultyConstants.DEFAULT_COMPLETION_TIME_SECONDS;
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
                this.logger?.Warning($"[UITemplateLevelProgressProvider] Error getting completion rate: {ex.Message}");
                return 0f;
            }
        }

    }
}