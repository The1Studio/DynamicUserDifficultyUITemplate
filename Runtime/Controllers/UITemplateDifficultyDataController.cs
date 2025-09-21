namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.UserData;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using UnityEngine.Scripting;

    /// <summary>
    /// UITemplate controller for Dynamic User Difficulty data.
    /// Implements all provider interfaces and manages difficulty state using UITemplate's data system.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyDataController : IUITemplateControllerData,
        IDifficultyDataProvider,
        IWinStreakProvider,
        ITimeDecayProvider,
        IRageQuitProvider,
        ILevelProgressProvider,
        ITickable
    {
        private readonly UITemplateDifficultyData difficultyData;
        private readonly SignalBus signalBus;
        private readonly IHandleUserDataServices handleUserDataServices;
        private readonly IDynamicDifficultyService difficultyService;
        private readonly UITemplateLevelDataController levelDataController;
        private readonly ILogger logger;

        private bool isSessionActive;
        private PlayerSessionData cachedSessionData;
        private bool dataCacheValid;

        [Preserve]
        public UITemplateDifficultyDataController(
            UITemplateDifficultyData difficultyData,
            SignalBus signalBus,
            IHandleUserDataServices handleUserDataServices,
            IDynamicDifficultyService difficultyService,
            UITemplateLevelDataController levelDataController,
            ILoggerManager loggerManager)
        {
            this.difficultyData = difficultyData;
            this.signalBus = signalBus;
            this.handleUserDataServices = handleUserDataServices;
            this.difficultyService = difficultyService;
            this.levelDataController = levelDataController;
            this.logger = loggerManager?.GetLogger(this);

            // Start session tracking
            this.RecordSessionStart();
        }

        #region IDifficultyDataProvider Implementation

        public float GetCurrentDifficulty()
        {
            return this.difficultyData.CurrentDifficulty;
        }

        public void SetCurrentDifficulty(float difficulty)
        {
            var clampedDifficulty = Math.Max(DifficultyConstants.MIN_DIFFICULTY,
                                            Math.Min(DifficultyConstants.MAX_DIFFICULTY, difficulty));

            this.difficultyData.CurrentDifficulty = clampedDifficulty;
            this.handleUserDataServices.Save();

            this.logger?.Info($"[UITemplateDifficultyController] Difficulty set to: {clampedDifficulty:F1}");
        }

        #endregion

        #region IWinStreakProvider Implementation

        public int GetWinStreak() => this.difficultyData.WinStreak;
        public int GetLossStreak() => this.difficultyData.LossStreak;
        public int GetTotalWins() => this.difficultyData.TotalWins;
        public int GetTotalLosses() => this.difficultyData.TotalLosses;

        public void RecordWin()
        {
            this.difficultyData.WinStreak++;
            this.difficultyData.LossStreak = 0;
            this.difficultyData.TotalWins++;
            this.InvalidateCache();
            this.handleUserDataServices.Save();

            // Recalculate difficulty
            this.RecalculateDifficulty();

            this.logger?.Info($"[UITemplateDifficultyController] Win recorded - Streak: {this.difficultyData.WinStreak}");
        }

        public void RecordLoss()
        {
            this.difficultyData.LossStreak++;
            this.difficultyData.WinStreak = 0;
            this.difficultyData.TotalLosses++;
            this.InvalidateCache();
            this.handleUserDataServices.Save();

            // Recalculate difficulty
            this.RecalculateDifficulty();

            this.logger?.Info($"[UITemplateDifficultyController] Loss recorded - Streak: {this.difficultyData.LossStreak}");
        }

        #endregion

        #region ITimeDecayProvider Implementation

        public DateTime GetLastPlayTime() => this.difficultyData.LastPlayTime;

        public TimeSpan GetTimeSinceLastPlay() => DateTime.Now - this.difficultyData.LastPlayTime;

        public void RecordPlaySession()
        {
            this.difficultyData.LastPlayTime = DateTime.Now;
            this.difficultyData.SessionStartTime = DateTime.Now;
            this.isSessionActive = true;
            this.handleUserDataServices.Save();
        }

        public int GetDaysAwayFromGame() => Math.Max(0, (int)this.GetTimeSinceLastPlay().TotalDays);

        #endregion

        #region IRageQuitProvider Implementation

        public QuitType GetLastQuitType() => this.difficultyData.LastQuitType;

        public float GetAverageSessionDuration()
        {
            // Could be enhanced with running average
            return this.difficultyData.LastSessionDuration;
        }

        public void RecordSessionEnd(QuitType quitType, float durationSeconds)
        {
            this.difficultyData.LastQuitType = quitType;
            this.difficultyData.LastSessionDuration = durationSeconds;
            this.isSessionActive = false;
            this.handleUserDataServices.Save();

            this.logger?.Info($"[UITemplateDifficultyController] Session ended: {quitType}, Duration: {durationSeconds:F1}s");
        }

        public float GetCurrentSessionDuration()
        {
            if (!this.isSessionActive)
                return this.difficultyData.LastSessionDuration;

            return (float)(DateTime.Now - this.difficultyData.SessionStartTime).TotalSeconds;
        }

        public int GetRecentRageQuitCount()
        {
            // Simple implementation - could track multiple recent quits
            return this.difficultyData.LastQuitType == QuitType.RageQuit ? 1 : 0;
        }

        public void RecordSessionStart()
        {
            this.RecordPlaySession();
        }

        #endregion

        #region ILevelProgressProvider Implementation

        public int GetCurrentLevel()
        {
            // Use UITemplate's level controller
            return this.levelDataController.CurrentLevel;
        }

        public float GetAverageCompletionTime() => this.difficultyData.LastCompletionTime;

        public int GetAttemptsOnCurrentLevel() => this.difficultyData.CurrentLevelAttempts;

        public float GetCompletionRate()
        {
            var totalAttempts = this.GetTotalWins() + this.GetTotalLosses();
            if (totalAttempts == 0) return 1.0f;
            return (float)this.GetTotalWins() / totalAttempts;
        }

        public void RecordLevelCompletion(int levelId, float completionTime, bool won)
        {
            this.difficultyData.LastCompletionTime = completionTime;

            if (won)
            {
                // Reset attempts on win
                this.difficultyData.CurrentLevelAttempts = 1;
            }
            else
            {
                // Increment attempts on loss
                this.difficultyData.CurrentLevelAttempts++;
            }

            this.handleUserDataServices.Save();
        }

        public float GetCurrentLevelDifficulty() => this.GetCurrentDifficulty();

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get session data for difficulty calculation
        /// </summary>
        public PlayerSessionData GetSessionData()
        {
            if (!this.dataCacheValid || this.cachedSessionData == null)
            {
                this.cachedSessionData = new PlayerSessionData
                {
                    WinStreak = this.GetWinStreak(),
                    LossStreak = this.GetLossStreak(),
                    TotalWins = this.GetTotalWins(),
                    TotalLosses = this.GetTotalLosses(),
                    CurrentDifficulty = this.GetCurrentDifficulty(),
                    LastPlayTime = this.GetLastPlayTime(),
                    QuitType = this.GetLastQuitType(),
                    SessionCount = this.GetTotalWins() + this.GetTotalLosses(),
                    RecentSessions = this.difficultyData.RecentSessions
                };
                this.dataCacheValid = true;
            }
            return this.cachedSessionData;
        }

        /// <summary>
        /// Recalculate difficulty based on current session data
        /// </summary>
        public void RecalculateDifficulty()
        {
            var currentDifficulty = this.GetCurrentDifficulty();
            var sessionData = this.GetSessionData();

            // Calculate new difficulty using stateless service
            var result = this.difficultyService.CalculateDifficulty(currentDifficulty, sessionData);

            // Store the new difficulty if it changed
            if (Math.Abs(result.NewDifficulty - currentDifficulty) > 0.01f)
            {
                this.SetCurrentDifficulty(result.NewDifficulty);
            }

            this.logger?.Info($"[UITemplateDifficultyController] Difficulty recalculated: {currentDifficulty:F1} -> {result.NewDifficulty:F1}");
        }

        /// <summary>
        /// Clear all difficulty data
        /// </summary>
        public void ClearData()
        {
            this.difficultyData.CurrentDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;
            this.difficultyData.WinStreak = 0;
            this.difficultyData.LossStreak = 0;
            this.difficultyData.TotalWins = 0;
            this.difficultyData.TotalLosses = 0;
            this.difficultyData.LastPlayTime = DateTime.Now;
            this.difficultyData.LastQuitType = QuitType.Normal;
            this.difficultyData.LastSessionDuration = 0f;
            this.difficultyData.CurrentLevelAttempts = 1;
            this.difficultyData.LastCompletionTime = 60f;
            this.difficultyData.RecentSessions.Clear();

            this.InvalidateCache();
            this.handleUserDataServices.Save();
        }

        private void InvalidateCache()
        {
            this.dataCacheValid = false;
            this.cachedSessionData = null;
        }

        #endregion

        #region ITickable Implementation

        public void Tick()
        {
            // Could be used for periodic difficulty adjustments or time-based decay
            // Currently not needed for basic implementation
        }

        #endregion
    }
}