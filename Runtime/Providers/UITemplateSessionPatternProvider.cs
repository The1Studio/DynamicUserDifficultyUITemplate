namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides advanced session pattern analysis data from UITemplate's session tracking.
    /// Implements detailed session history analysis for SessionPatternModifier.
    /// </summary>
    [Preserve]
    public class UITemplateSessionPatternProvider : ISessionPatternProvider
    {
        private readonly UITemplateGameSessionDataController sessionController;
        private readonly UITemplateDifficultyDataController difficultyController;

        [Preserve]
        public UITemplateSessionPatternProvider(
            UITemplateGameSessionDataController sessionController,
            UITemplateDifficultyDataController difficultyController)
        {
            this.sessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));
            this.difficultyController = difficultyController ?? throw new ArgumentNullException(nameof(difficultyController));
        }

        /// <summary>
        /// Gets the recent session durations from UITemplate's detailed history
        /// </summary>
        public List<float> GetRecentSessionDurations(int count)
        {
            try
            {
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count == 0)
                {
                    return new();
                }

                // Get the most recent sessions up to requested count
                var recentCount = Math.Min(count, sessionHistory.Count);
                var durations = new List<float>(recentCount);

                // Extract durations from most recent sessions
                for (var i = sessionHistory.Count - recentCount; i < sessionHistory.Count; i++)
                {
                    durations.Add(sessionHistory[i].Duration);
                }

                return durations;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateSessionPatternProvider] Error getting recent session durations: {ex.Message}");
                return new();
            }
        }

        /// <summary>
        /// Gets the total number of recent quits by counting all sessions
        /// </summary>
        public int GetTotalRecentQuits()
        {
            try
            {
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count == 0)
                {
                    return 0;
                }

                // Count sessions within the recent window (last 10 sessions)
                var maxSessionsToCheck = Math.Min(DifficultyConstants.MAX_RECENT_SESSIONS, sessionHistory.Count);
                return maxSessionsToCheck;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateSessionPatternProvider] Error getting total recent quits: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the number of mid-level quits from recent sessions
        /// </summary>
        public int GetRecentMidLevelQuits()
        {
            try
            {
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count == 0)
                {
                    return 0;
                }

                var midLevelQuitCount = 0;
                var maxSessionsToCheck = Math.Min(DifficultyConstants.MAX_RECENT_SESSIONS, sessionHistory.Count);

                for (var i = sessionHistory.Count - maxSessionsToCheck; i < sessionHistory.Count; i++)
                {
                    var session = sessionHistory[i];

                    // Determine if this was a mid-level quit
                    // Mid-level quit: session ended during active play (not too short, not completed)
                    var notTooShort = session.Duration >= DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD;
                    var notNormalLength = session.Duration < DifficultyConstants.NORMAL_SESSION_THRESHOLD_SECONDS;
                    var endedMidLevel = session.TimeSinceLastLevel < DifficultyConstants.SHORT_SESSION_THRESHOLD_SECONDS;

                    if (notTooShort && notNormalLength && endedMidLevel && !session.LastLevelWon)
                    {
                        midLevelQuitCount++;
                    }
                }

                return midLevelQuitCount;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateSessionPatternProvider] Error getting recent mid-level quits: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the difficulty value from before the last adjustment
        /// Uses the difficulty tracking data from the controller
        /// </summary>
        public float GetPreviousDifficulty()
        {
            try
            {
                // Get previous difficulty from the controller's history
                return this.difficultyController?.GetPreviousDifficulty() ?? 0f;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateSessionPatternProvider] Error getting previous difficulty: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Gets the session duration from before the last difficulty adjustment
        /// </summary>
        public float GetSessionDurationBeforeLastAdjustment()
        {
            try
            {
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count < 2)
                {
                    return 0f;
                }

                // Get the second-to-last session (before the adjustment)
                return sessionHistory[^2].Duration;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateSessionPatternProvider] Error getting session duration before last adjustment: {ex.Message}");
                return 0f;
            }
        }
    }
}