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
    /// Provides access to the current difficulty value for game systems.
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

            // Session tracking is handled by the controller

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

            // Use centralized quit type determination from the core service
            var quitType = this.difficultyController.GetLastQuitType();

            this.difficultyController.RecordSessionEnd(quitType, sessionDuration);

            this.logger?.Info($"[UITemplateDifficultyAdapter] Application quit - Session duration: {sessionDuration:F1}s");
        }

        private void OnApplicationPause(ApplicationPauseSignal signal)
        {
            if (signal.PauseStatus)
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
                if (timeSinceLastPlay.TotalHours > DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS)
                {
                    // Recalculate difficulty with time decay
                    this.difficultyController.RecalculateDifficulty();
                    this.logger?.Info($"[UITemplateDifficultyAdapter] Player returned after {timeSinceLastPlay.TotalHours:F1} hours - Difficulty adjusted to: {this.CurrentDifficulty:F1}");
                }
            }
        }

        /// <summary>
        /// Get current difficulty value (1-10 scale)
        /// </summary>
        public float CurrentDifficulty => this.difficultyController.GetCurrentDifficulty();

        public void Dispose()
        {
            this.signalBus?.TryUnsubscribe<LevelStartedSignal>(this.OnLevelStarted);
            this.signalBus?.TryUnsubscribe<LevelEndedSignal>(this.OnLevelEnded);
            this.signalBus?.TryUnsubscribe<LevelSkippedSignal>(this.OnLevelSkipped);
            this.signalBus?.TryUnsubscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus?.TryUnsubscribe<ApplicationPauseSignal>(this.OnApplicationPause);
        }
    }
}