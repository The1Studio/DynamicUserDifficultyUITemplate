namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides time-based decay information from UITemplate's session tracking.
    /// Handles time since last play for difficulty adjustment.
    /// </summary>
    [Preserve]
    public class UITemplateTimeDecayProvider : ITimeDecayProvider
    {
        private readonly UITemplateGameSessionDataController sessionController;

        [Preserve]
        public UITemplateTimeDecayProvider(UITemplateGameSessionDataController sessionController)
        {
            this.sessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));
        }

        /// <summary>
        /// Gets the time elapsed since the last play session.
        /// </summary>
        public TimeSpan GetTimeSinceLastPlay()
        {
            try
            {
                // Get the last session end time from session history
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                // Get the most recent session's end time
                var lastSession = sessionHistory[sessionHistory.Count - 1];
                var timeSinceLastPlay = DateTime.UtcNow - lastSession.EndTime;

                // Ensure we don't return negative time spans
                return timeSinceLastPlay > TimeSpan.Zero ? timeSinceLastPlay : TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateTimeDecayProvider] Error getting time since last play: {ex.Message}");
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Gets the last time the player played.
        /// </summary>
        public DateTime GetLastPlayTime()
        {
            try
            {
                var sessionHistory = this.sessionController?.GetDetailedSessionHistory();
                if (sessionHistory == null || sessionHistory.Count == 0)
                {
                    return DateTime.UtcNow.AddDays(-1); // Default to 1 day ago if no history
                }

                return sessionHistory[sessionHistory.Count - 1].EndTime;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateTimeDecayProvider] Error getting last session end time: {ex.Message}");
                return DateTime.UtcNow.AddDays(-1);
            }
        }

        /// <summary>
        /// Gets the number of consecutive days without playing.
        /// </summary>
        public int GetDaysAwayFromGame()
        {
            try
            {
                var timeSinceLastPlay = GetTimeSinceLastPlay();
                return Math.Max(0, (int)timeSinceLastPlay.TotalDays);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UITemplateTimeDecayProvider] Error getting days away from game: {ex.Message}");
                return 0;
            }
        }
    }
}