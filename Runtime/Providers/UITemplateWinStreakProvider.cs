namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using System.Linq;
    using TheOne.Features.WinStreak.Core.Controller;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides win/loss streak data from UITemplate's level progression system.
    /// Reads data from existing UITemplate controllers without duplication.
    /// </summary>
    [Preserve]
    public class UITemplateWinStreakProvider : IWinStreakProvider
    {
        private readonly UITemplateLevelDataController levelController;
        private readonly WinStreakLocalDataController winStreakController;

        [Preserve]
        public UITemplateWinStreakProvider(
            UITemplateLevelDataController levelController,
            WinStreakLocalDataController winStreakController)
        {
            this.levelController = levelController ?? throw new ArgumentNullException(nameof(levelController));
            this.winStreakController = winStreakController ?? throw new ArgumentNullException(nameof(winStreakController));
        }

        /// <summary>
        /// Gets the current win streak from the win streak controller.
        /// </summary>
        public int GetWinStreak()
        {
            // Use WinStreakLocalDataController which has the Streak property
            var winStreak = this.winStreakController?.Streak ?? 0;
            var lossStreak = this.winStreakController?.LossStreak ?? 0;
            Debug.Log($"[UITemplateWinStreakProvider] GetWinStreak() → Win:{winStreak}, Loss:{lossStreak}");
            Debug.Log($"[UITemplateWinStreakProvider] ⚠️  NOTE: Streak may be 0 if LoseStreakViaPlay() was called at level start!");
            return winStreak;
        }

        /// <summary>
        /// Gets the current loss streak from the win streak controller.
        /// </summary>
        public int GetLossStreak()
        {
            // Use WinStreakLocalDataController which has the LossStreak property
            var lossStreak = this.winStreakController?.LossStreak ?? 0;
            Debug.Log($"[UITemplateWinStreakProvider] GetLossStreak() → {lossStreak}");
            return lossStreak;
        }

        /// <summary>
        /// Gets the total number of wins across all sessions.
        /// </summary>
        public int GetTotalWins()
        {
            // Sum up all wins from level data
            var allLevels = this.levelController?.GetAllLevelData().ToList();
            if (allLevels == null) return 0;

            return allLevels.Sum(level => level.WinCount);
        }

        /// <summary>
        /// Gets the total number of losses across all sessions.
        /// </summary>
        public int GetTotalLosses()
        {
            // Sum up all losses from level data
            var allLevels = this.levelController?.GetAllLevelData().ToList();
            if (allLevels == null) return 0;

            return allLevels.Sum(level => level.LoseCount);
        }
    }
}