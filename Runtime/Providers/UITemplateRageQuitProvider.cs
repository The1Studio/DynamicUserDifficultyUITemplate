namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides rage quit detection data from UITemplate's session tracking.
    /// Analyzes session patterns to identify frustration-based quits.
    /// </summary>
    [Preserve]
    public class UITemplateRageQuitProvider : IRageQuitProvider
    {
        private readonly UITemplateGameSessionDataController sessionController;
        private readonly UITemplateLevelDataController levelController;

        [Preserve]
        public UITemplateRageQuitProvider(
            UITemplateGameSessionDataController sessionController,
            UITemplateLevelDataController levelController)
        {
            this.sessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));
            this.levelController = levelController ?? throw new ArgumentNullException(nameof(levelController));
        }

        /// <summary>
        /// Gets the type of the last quit (normal, rage quit, etc.)
        /// </summary>
        public QuitType GetLastQuitType()
        {
            try
            {
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count == 0)
                {
                    return QuitType.Normal;
                }

                var lastSession = sessionHistory[^1];

                // Determine quit type based on session characteristics
                var shortSession = lastSession.Duration < DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD;
                var hadLosses = lastSession.LevelsFailed > 0;
                var noWins = lastSession.LevelsCompleted == 0;
                var endedAfterLoss = !lastSession.LastLevelWon && lastSession.TimeSinceLastLevel < DifficultyConstants.SHORT_SESSION_THRESHOLD_SECONDS;

                if (shortSession && hadLosses && (noWins || endedAfterLoss))
                {
                    return QuitType.RageQuit;
                }
                else if (lastSession.Duration > DifficultyConstants.NORMAL_SESSION_THRESHOLD_SECONDS)
                {
                    return QuitType.Normal;
                }
                else
                {
                    return QuitType.MidPlay;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateRageQuitProvider] Error detecting quit type: {ex.Message}");
                return QuitType.Normal;
            }
        }

        /// <summary>
        /// Gets the average session duration for recent sessions.
        /// </summary>
        public float GetAverageSessionDuration()
        {
            try
            {
                // Use the existing method from session controller
                return this.sessionController?.GetAverageSessionDuration() ?? DifficultyConstants.DEFAULT_SESSION_DURATION_SECONDS;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateRageQuitProvider] Error getting average session duration: {ex.Message}");
                return DifficultyConstants.DEFAULT_SESSION_DURATION_SECONDS;
            }
        }

        /// <summary>
        /// Gets the time spent on current session.
        /// </summary>
        public float GetCurrentSessionDuration()
        {
            try
            {
                var sessionStartTime = this.sessionController?.SessionStartTime ?? DateTime.UtcNow;
                var currentDuration = (float)(DateTime.UtcNow - sessionStartTime).TotalSeconds;
                return currentDuration;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateRageQuitProvider] Error getting current session duration: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Gets the number of rage quits in recent history.
        /// </summary>
        public int GetRecentRageQuitCount()
        {
            try
            {
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count == 0)
                {
                    return 0;
                }

                // Count rage quits from recent sessions
                var rageQuitCount = 0;
                var maxSessionsToCheck = Math.Min(DifficultyConstants.MAX_RECENT_SESSIONS, sessionHistory.Count);

                for (var i = sessionHistory.Count - maxSessionsToCheck; i < sessionHistory.Count; i++)
                {
                    var session = sessionHistory[i];
                    var isRageQuit = session.Duration < DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD &&
                                     session.LevelsFailed > 0 &&
                                     session.LevelsCompleted == 0;

                    if (isRageQuit)
                    {
                        rageQuitCount++;
                    }
                }

                return rageQuitCount;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateRageQuitProvider] Error getting recent rage quit count: {ex.Message}");
                return 0;
            }
        }
    }
}