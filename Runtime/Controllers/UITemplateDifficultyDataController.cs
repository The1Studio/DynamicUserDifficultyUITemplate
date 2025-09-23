namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.ApplicationServices;
    using GameFoundation.Scripts.Utilities.UserData;
    using GameFoundation.Signals;
    using TheOne.Features.WinStreak.Core.Controller;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Models.LocalDatas;
    using TheOneStudio.UITemplate.UITemplate.Signals;
    using UnityEngine.Scripting;

    /// <summary>
    /// UITemplate controller for Dynamic User Difficulty data.
    /// Implements all provider interfaces and manages difficulty state using UITemplate's data system.
    /// Uses existing UITemplate/TheOne controllers for data that's already tracked.
    /// All data is READ-ONLY from existing UITemplate/TheOne systems.
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
        private readonly UITemplateDifficultyData  difficultyData;
        private readonly SignalBus                 signalBus;
        private readonly IDynamicDifficultyService difficultyService;

        // Existing controllers for data we don't duplicate
        private readonly UITemplateLevelDataController levelDataController;
        private readonly WinStreakLocalDataController winStreakController;
        private readonly UITemplateGameSessionDataController gameSessionController;

        private readonly ILogger logger;

        private PlayerSessionData cachedSessionData;
        private bool dataCacheValid;

        [Preserve]
        public UITemplateDifficultyDataController(
            UITemplateDifficultyData difficultyData,
            SignalBus signalBus,
            IHandleUserDataServices handleUserDataServices,
            IDynamicDifficultyService difficultyService,
            UITemplateLevelDataController levelDataController,
            WinStreakLocalDataController winStreakController,
            UITemplateGameSessionDataController gameSessionController,
            ILoggerManager loggerManager)
        {
            this.difficultyData        = difficultyData;
            this.signalBus             = signalBus;
            this.difficultyService     = difficultyService;
            this.levelDataController   = levelDataController;
            this.winStreakController   = winStreakController;
            this.gameSessionController = gameSessionController;
            this.logger                = loggerManager?.GetLogger(this);

            // Start session tracking
            this.RecordSessionStart();

            // Subscribe to application lifecycle events
            this.signalBus.Subscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus.Subscribe<ApplicationPauseSignal>(this.OnApplicationPause);

            // Subscribe to level events
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);
            this.signalBus.Subscribe<LevelStartedSignal>(this.OnLevelStarted);
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

            this.logger?.Info($"[UITemplateDifficultyController] Difficulty set to: {clampedDifficulty:F1}");
        }

        #endregion

        #region IWinStreakProvider Implementation

        // Use existing WinStreakController for win streak data
        public int GetWinStreak() => this.winStreakController.Streak;

        // Loss streak is tracked by WinStreakController
        public int GetLossStreak() => this.winStreakController.LossStreak;

        // Use existing UITemplateLevelDataController for total wins/losses
        public int GetTotalWins() => this.levelDataController.TotalWin;
        public int GetTotalLosses() => this.levelDataController.TotalLose;

        public void RecordWin()
        {
            // Both win streak and loss streak are handled by WinStreakController
            // Loss streak will be reset automatically by WinStreakController
            this.InvalidateCache();

            // Recalculate difficulty
            this.RecalculateDifficulty();

            this.logger?.Info($"[UITemplateDifficultyController] Win recorded - Win Streak: {this.GetWinStreak()}");
        }

        public void RecordLoss()
        {
            // Both loss streak increment and win streak reset
            // are handled automatically by WinStreakController
            this.InvalidateCache();

            // Recalculate difficulty
            this.RecalculateDifficulty();

            this.logger?.Info($"[UITemplateDifficultyController] Loss recorded - Loss Streak: {this.GetLossStreak()}");
        }

        #endregion

        #region ITimeDecayProvider Implementation

        public DateTime GetLastPlayTime()
        {
            // Check if we have LoseTime data first
            var currentLevelData = this.levelDataController.GetCurrentLevelData;
            if (currentLevelData.LoseTime != null && currentLevelData.LoseTime.Count > 0)
            {
                // Get the most recent lose time (last element in the list)
                var lastLoseTimeUnix = currentLevelData.LoseTime[currentLevelData.LoseTime.Count - 1];
                return DateTimeOffset.FromUnixTimeSeconds(lastLoseTimeUnix).DateTime;
            }

            // Fallback to last level end time
            return this.gameSessionController.LastLevelEndTime;
        }

        public TimeSpan GetTimeSinceLastPlay()
        {
            return DateTime.Now - this.GetLastPlayTime();
        }

        public void RecordPlaySession()
        {
            // Do nothing - UITemplate manages session tracking
            // This is just for interface compliance
        }

        public int GetDaysAwayFromGame() => Math.Max(0, (int)this.GetTimeSinceLastPlay().TotalDays);

        #endregion

        #region IRageQuitProvider Implementation

        public QuitType GetLastQuitType()
        {
            // Use centralized utility method from DynamicDifficultyService
            var lastDuration = this.gameSessionController.LastSessionDuration;
            var wasLastLevelWon = this.gameSessionController.WasLastLevelWon;
            var lastLevelEndTime = this.gameSessionController.LastLevelEndTime;

            return DynamicDifficultyService.DetermineQuitType(lastDuration, wasLastLevelWon, lastLevelEndTime);
        }

        public float GetAverageSessionDuration()
        {
            // Now we have session history in GameSessionDataController
            return this.gameSessionController.GetAverageSessionDuration();
        }

        public void RecordSessionEnd(QuitType quitType, float durationSeconds)
        {
            // Do nothing - UITemplate manages session tracking
            // This is just for interface compliance
            this.logger?.Info($"[UITemplateDifficultyController] Session ended: {quitType}, Duration: {durationSeconds:F1}s");
        }

        public float GetCurrentSessionDuration()
        {
            // Always calculate from UITemplate's session data
            return (float)(DateTime.Now - this.gameSessionController.SessionStartTime).TotalSeconds;
        }

        public int GetRecentRageQuitCount()
        {
            // Check if the last quit was a rage quit
            // UITemplate doesn't track rage quit history, only last quit
            return this.GetLastQuitType() == QuitType.RageQuit ? 1 : 0;
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

        public float GetAverageCompletionTime()
        {
            // Get from UITemplateLevelDataController which calculates from all levels with WinTime > 0
            return this.levelDataController.GetAverageCompletionTime();
        }

        public int GetAttemptsOnCurrentLevel()
        {
            // Get from UITemplateLevelDataController: LoseCount + 1
            return this.levelDataController.GetAttemptsOnCurrentLevel();
        }

        public float GetCompletionRate()
        {
            var totalAttempts = this.GetTotalWins() + this.GetTotalLosses();
            if (totalAttempts == 0) return 1.0f;
            return (float)this.GetTotalWins() / totalAttempts;
        }

        public void RecordLevelCompletion(int levelId, float completionTime, bool won)
        {
            // We don't store level completion data - UITemplate handles that
            // Just trigger win/loss recording for difficulty calculation
            if (won)
            {
                this.RecordWin();
            }
            else
            {
                this.RecordLoss();
            }
        }

        public float GetCurrentLevelDifficulty()
        {
            // Get the static level difficulty from LevelData
            var currentLevelData = this.levelDataController.GetCurrentLevelData;

            // Convert enum to float value (Easy=1, Normal=2, Hard=3, VeryHard=4)
            var staticDifficulty = (float)(currentLevelData.StaticDifficulty + 1);

            // Also check if there's a DynamicDifficulty value set
            if (currentLevelData.DynamicDifficulty > 0)
            {
                return currentLevelData.DynamicDifficulty;
            }

            return staticDifficulty;
        }

        /// <summary>
        /// Gets the average percent time to complete from UITemplate level data
        /// </summary>
        public float GetCompletionTimePercentage()
        {
            // Get from UITemplateLevelDataController which calculates from all levels with PercentUsingTimeToComplete > 0
            return this.levelDataController.GetAveragePercentTimeToComplete();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get session data for difficulty calculation
        /// </summary>
        public PlayerSessionData GetSessionData()
        {
            if (!this.dataCacheValid || this.cachedSessionData == null)
            {
                this.cachedSessionData = new()
                {
                    WinStreak         = this.GetWinStreak(),
                    LossStreak        = this.GetLossStreak(),
                    TotalWins         = this.GetTotalWins(),
                    TotalLosses       = this.GetTotalLosses(),
                    CurrentDifficulty = this.GetCurrentDifficulty(),
                    LastPlayTime      = this.GetLastPlayTime(),
                    QuitType          = this.GetLastQuitType(),
                    SessionCount      = this.GetTotalWins() + this.GetTotalLosses(),
                    RecentSessions    = new(), // Empty queue for now
                    DetailedSessions  = this.GetDetailedSessionHistory()
                };
                this.dataCacheValid = true;
            }
            return this.cachedSessionData;
        }

        /// <summary>
        /// Get detailed session history from GameSessionDataController
        /// </summary>
        private List<DetailedSessionInfo> GetDetailedSessionHistory()
        {
            // Get type-safe raw session history from UITemplate and convert to DetailedSessionInfo
            if (this.gameSessionController != null)
            {
                var rawSessions = this.gameSessionController.GetDetailedSessionHistory();
                var detailedSessions = new List<DetailedSessionInfo>();

                foreach (var rawSession in rawSessions)
                {
                    // Convert type-safe RawSessionData to DetailedSessionInfo
                    var sessionInfo = new DetailedSessionInfo
                    {
                        StartTime = rawSession.StartTime,
                        EndTime = rawSession.EndTime,
                        Duration = rawSession.Duration,
                        LevelsCompleted = rawSession.LevelsCompleted,
                        LevelsFailed = rawSession.LevelsFailed,
                        LastLevelPlayed = rawSession.LastLevelPlayed,
                        LastLevelWon = rawSession.LastLevelWon,
                        StartDifficulty = rawSession.StartDifficulty,
                        EndDifficulty = rawSession.EndDifficulty,

                        // Let DynamicDifficultyService determine quit type from raw data
                        QuitType = DetermineQuitTypeFromRawData(rawSession)
                    };

                    detailedSessions.Add(sessionInfo);
                }

                return detailedSessions;
            }
            return new();
        }

        /// <summary>
        /// Determine quit type from type-safe raw session data using DynamicDifficultyService logic
        /// </summary>
        private QuitType DetermineQuitTypeFromRawData(RawSessionData rawSession)
        {
            // Use centralized quit type determination from DynamicDifficultyService
            return DynamicDifficultyService.DetermineQuitType(
                rawSession.Duration,
                rawSession.LastLevelWon,
                rawSession.LastLevelEndTime);
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

        #region Signal Handlers

        private void OnApplicationQuit(ApplicationQuitSignal signal)
        {
            // Record session end when app quits
            var sessionDuration = this.GetCurrentSessionDuration();
            var quitType = this.GetLastQuitType();
            this.RecordSessionEnd(quitType, sessionDuration);
        }

        private void OnApplicationPause(ApplicationPauseSignal signal)
        {
            if (signal.PauseStatus)
            {
                // App going to background - record session end
                var sessionDuration = this.GetCurrentSessionDuration();
                var quitType = this.GetLastQuitType();
                this.RecordSessionEnd(quitType, sessionDuration);
            }
            else
            {
                // App returning from background - start new session
                this.RecordSessionStart();
            }
        }

        private void OnLevelStarted(LevelStartedSignal signal)
        {
            // Track when a new level starts - helps with attempt counting
            this.logger?.Info($"[UITemplateDifficultyController] Level {signal.Level} started");
        }

        private void OnLevelEnded(LevelEndedSignal signal)
        {
            // Just trigger win/loss recording for difficulty calculation
            // UITemplate already tracks all the level data
            this.RecordLevelCompletion(signal.Level, 0, signal.IsWin);

            var percentage = this.GetCompletionTimePercentage();
            this.logger?.Info($"[UITemplateDifficultyController] Level {signal.Level} ended - Win: {signal.IsWin}, Average completion: {percentage:F0}%");
        }

        #endregion
    }
}