namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        [Preserve]
        public UITemplateWinStreakProvider(UITemplateLevelDataController levelController)
        {
            this.levelController = levelController ?? throw new ArgumentNullException(nameof(levelController));
        }

        /// <summary>
        /// Gets the current win streak from the level controller.
        /// </summary>
        public int GetWinStreak()
        {
            // UITemplate tracks consecutive wins - correct property name
            return this.levelController?.WinSteak ?? 0;
        }

        /// <summary>
        /// Gets the current loss streak from the level controller.
        /// </summary>
        public int GetLossStreak()
        {
            // UITemplate tracks consecutive losses - correct property name
            return this.levelController?.LoseSteak ?? 0;
        }

        /// <summary>
        /// Gets the total number of wins across all sessions.
        /// </summary>
        public int GetTotalWins()
        {
            // Sum up all wins from level data
            var allLevels = this.levelController?.GetAllLevels();
            if (allLevels == null) return 0;

            return allLevels.Sum(level => level.WinCount);
        }

        /// <summary>
        /// Gets the total number of losses across all sessions.
        /// </summary>
        public int GetTotalLosses()
        {
            // Sum up all losses from level data
            var allLevels = this.levelController?.GetAllLevels();
            if (allLevels == null) return 0;

            return allLevels.Sum(level => level.LoseCount);
        }
    }
}