namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers
{
    using System;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides time-based decay information from UITemplate's session tracking.
    /// Handles time since last play for difficulty adjustment.
    /// </summary>
    [Preserve]
    public class UITemplateTimeDecayProvider : ITimeDecayProvider
    {
        private readonly UITemplateGameSessionDataController sessionController;
        private readonly ILogger logger;

        [Preserve]
        public UITemplateTimeDecayProvider(UITemplateGameSessionDataController sessionController, ILogger logger)
        {
            this.sessionController = sessionController ?? throw new ArgumentNullException(nameof(sessionController));
            this.logger = logger;
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
                var lastSession       = sessionHistory[^1];
                var timeSinceLastPlay = DateTime.UtcNow - lastSession.EndTime;

                // Ensure we don't return negative time spans
                return timeSinceLastPlay > TimeSpan.Zero ? timeSinceLastPlay : TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                this.logger?.Warning($"[UITemplateTimeDecayProvider] Error getting time since last play: {ex.Message}");
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
                    return DateTime.UtcNow.AddDays(-UITemplateIntegrationConstants.DEFAULT_DAYS_AGO); // Default to 1 day ago if no history
                }

                return sessionHistory[^1].EndTime;
            }
            catch (Exception ex)
            {
                this.logger?.Warning($"[UITemplateTimeDecayProvider] Error getting last session end time: {ex.Message}");
                return DateTime.UtcNow.AddDays(-UITemplateIntegrationConstants.DEFAULT_DAYS_AGO);
            }
        }

        /// <summary>
        /// Gets the number of consecutive days without playing.
        /// </summary>
        public int GetDaysAwayFromGame()
        {
            try
            {
                var timeSinceLastPlay = this.GetTimeSinceLastPlay();
                return Math.Max(0, (int)timeSinceLastPlay.TotalDays);
            }
            catch (Exception ex)
            {
                this.logger?.Warning($"[UITemplateTimeDecayProvider] Error getting days away from game: {ex.Message}");
                return 0;
            }
        }
    }
}