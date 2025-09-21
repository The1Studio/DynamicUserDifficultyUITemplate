namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.ApplicationServices;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Signals;
    using UnityEngine.Scripting;

    /// <summary>
    /// Adapter that connects UITemplate signals to Dynamic User Difficulty system.
    /// Automatically tracks game events and updates difficulty.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyAdapter : IInitializable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly IDynamicDifficultyService difficultyService;
        private readonly UITemplateDifficultyDataController difficultyController;
        private readonly ILogger logger;

        private DateTime levelStartTime;
        private int currentLevelId;

        [Preserve]
        public UITemplateDifficultyAdapter(
            SignalBus signalBus,
            IDynamicDifficultyService difficultyService,
            UITemplateDifficultyDataController difficultyController,
            ILoggerManager loggerManager)
        {
            this.signalBus = signalBus;
            this.difficultyService = difficultyService;
            this.difficultyController = difficultyController;
            this.logger = loggerManager?.GetLogger(this);
        }

        public void Initialize()
        {
            // Subscribe to UITemplate signals
            this.signalBus.Subscribe<LevelStartedSignal>(this.OnLevelStarted);
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);
            this.signalBus.Subscribe<LevelSkippedSignal>(this.OnLevelSkipped);
            this.signalBus.Subscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus.Subscribe<ApplicationPauseSignal>(this.OnApplicationPause);

            // Start session tracking
            this.difficultyController.RecordSessionStart();

            this.logger?.Info("[UITemplateDifficultyAdapter] Initialized and subscribed to signals");
        }

        private void OnLevelStarted(LevelStartedSignal signal)
        {
            this.levelStartTime = DateTime.Now;
            this.currentLevelId = signal.Level;

            // Update current level attempts
            if (this.currentLevelId != this.difficultyController.GetCurrentLevel())
            {
                // New level, reset attempts
                this.difficultyController.RecordLevelCompletion(signal.Level, 0f, false);
            }

            this.logger?.Info($"[UITemplateDifficultyAdapter] Level {signal.Level} started, Difficulty: {this.CurrentDifficulty:F1}");
        }

        private void OnLevelEnded(LevelEndedSignal signal)
        {
            var completionTime = (float)(DateTime.Now - this.levelStartTime).TotalSeconds;

            if (signal.IsWin)
            {
                // Record win
                this.difficultyController.RecordWin();
                this.difficultyController.RecordLevelCompletion(signal.Level, completionTime, true);

                this.logger?.Info($"[UITemplateDifficultyAdapter] Level {signal.Level} won in {completionTime:F1}s");
            }
            else
            {
                // Record loss
                this.difficultyController.RecordLoss();
                this.difficultyController.RecordLevelCompletion(signal.Level, completionTime, false);

                // Check for potential rage quit (very quick loss)
                if (completionTime < DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD)
                {
                    this.difficultyController.RecordSessionEnd(QuitType.RageQuit, completionTime);
                }

                this.logger?.Info($"[UITemplateDifficultyAdapter] Level {signal.Level} lost in {completionTime:F1}s");
            }

            // Add session info for tracking
            var sessionInfo = new SessionInfo
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = this.levelStartTime,
                EndTime = DateTime.Now,
                DurationSeconds = completionTime,
                LevelsPlayed = 1,
                LevelsPassed = signal.IsWin ? 1 : 0,
                DifficultyAtStart = this.CurrentDifficulty,
                DifficultyAtEnd = this.CurrentDifficulty
            };

            // Keep only recent sessions (last 10)
            var recentSessions = this.difficultyController.GetSessionData().RecentSessions;
            while (recentSessions.Count >= 10)
            {
                recentSessions.Dequeue();
            }
            recentSessions.Enqueue(sessionInfo);

            this.logger?.Info($"[UITemplateDifficultyAdapter] Updated difficulty after level {signal.Level}: {this.CurrentDifficulty:F1}");
        }

        private void OnLevelSkipped(LevelSkippedSignal signal)
        {
            // Skipping counts as a loss for difficulty purposes
            this.difficultyController.RecordLoss();

            var completionTime = (float)(DateTime.Now - this.levelStartTime).TotalSeconds;
            this.difficultyController.RecordLevelCompletion(signal.Level, completionTime, false);

            this.logger?.Info($"[UITemplateDifficultyAdapter] Level {signal.Level} skipped - treated as loss");
        }

        private void OnApplicationQuit(ApplicationQuitSignal signal)
        {
            var sessionDuration = this.difficultyController.GetCurrentSessionDuration();

            // Determine quit type based on session duration and recent activity
            var quitType = sessionDuration < 60f ? QuitType.RageQuit : QuitType.Normal;

            this.difficultyController.RecordSessionEnd(quitType, sessionDuration);

            this.logger?.Info($"[UITemplateDifficultyAdapter] Application quit - Session duration: {sessionDuration:F1}s");
        }

        private void OnApplicationPause(ApplicationPauseSignal signal)
        {
            if (signal.IsPaused)
            {
                // App going to background
                var sessionDuration = this.difficultyController.GetCurrentSessionDuration();
                this.difficultyController.RecordSessionEnd(QuitType.Normal, sessionDuration);
            }
            else
            {
                // App returning from background
                this.difficultyController.RecordSessionStart();

                // Check time decay if player was away for a while
                var timeSinceLastPlay = this.difficultyController.GetTimeSinceLastPlay();
                if (timeSinceLastPlay.TotalHours > 6)
                {
                    // Recalculate difficulty with time decay
                    this.difficultyController.RecalculateDifficulty();
                    this.logger?.Info($"[UITemplateDifficultyAdapter] Player returned after {timeSinceLastPlay.TotalHours:F1} hours - Difficulty adjusted to: {this.CurrentDifficulty:F1}");
                }
            }
        }

        /// <summary>
        /// Get current difficulty level
        /// </summary>
        public float CurrentDifficulty => this.difficultyController.GetCurrentDifficulty();

        /// <summary>
        /// Get game parameters adjusted by current difficulty
        /// </summary>
        public GameParameters GetAdjustedParameters()
        {
            var difficulty = this.CurrentDifficulty;

            return new GameParameters
            {
                // Scale 1-10 difficulty to game parameters
                TimeLimit = UnityEngine.Mathf.Lerp(120f, 60f, (difficulty - 1f) / 9f), // 120s easy -> 60s hard
                HintsAvailable = UnityEngine.Mathf.RoundToInt(UnityEngine.Mathf.Lerp(5f, 1f, (difficulty - 1f) / 9f)), // 5 easy -> 1 hard
                ScoreMultiplier = UnityEngine.Mathf.Lerp(1f, 2f, (difficulty - 1f) / 9f), // 1x easy -> 2x hard
                EnemySpeed = UnityEngine.Mathf.Lerp(0.5f, 1.5f, (difficulty - 1f) / 9f), // 0.5x easy -> 1.5x hard
                EnemyHealth = UnityEngine.Mathf.Lerp(50f, 150f, (difficulty - 1f) / 9f), // 50 easy -> 150 hard
                PowerUpFrequency = UnityEngine.Mathf.Lerp(1.5f, 0.5f, (difficulty - 1f) / 9f) // 1.5x easy -> 0.5x hard
            };
        }

        /// <summary>
        /// Get difficulty level category
        /// </summary>
        public DifficultyLevel GetDifficultyLevel()
        {
            var difficulty = this.CurrentDifficulty;

            if (difficulty <= 3.5f) return DifficultyLevel.Easy;
            if (difficulty <= 6.5f) return DifficultyLevel.Medium;
            return DifficultyLevel.Hard;
        }

        /// <summary>
        /// Manual difficulty recalculation
        /// </summary>
        public void RecalculateDifficulty()
        {
            this.difficultyController.RecalculateDifficulty();
        }

        /// <summary>
        /// Reset difficulty to default
        /// </summary>
        public void ResetDifficulty()
        {
            this.difficultyController.ClearData();
            this.logger?.Info("[UITemplateDifficultyAdapter] Difficulty reset to default");
        }

        public void Dispose()
        {
            this.signalBus?.TryUnsubscribe<LevelStartedSignal>(this.OnLevelStarted);
            this.signalBus?.TryUnsubscribe<LevelEndedSignal>(this.OnLevelEnded);
            this.signalBus?.TryUnsubscribe<LevelSkippedSignal>(this.OnLevelSkipped);
            this.signalBus?.TryUnsubscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus?.TryUnsubscribe<ApplicationPauseSignal>(this.OnApplicationPause);
        }
    }

    /// <summary>
    /// Game parameters that can be adjusted by difficulty
    /// </summary>
    public struct GameParameters
    {
        public float TimeLimit;
        public int HintsAvailable;
        public float ScoreMultiplier;
        public float EnemySpeed;
        public float EnemyHealth;
        public float PowerUpFrequency;
    }

    /// <summary>
    /// Difficulty level categories
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }
}