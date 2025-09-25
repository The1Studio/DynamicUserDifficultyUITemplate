namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.ApplicationServices;
    using GameFoundation.Scripts.Utilities.UserData;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Signals;
    using UnityEngine.Scripting;

    /// <summary>
    /// UITemplate controller for Dynamic User Difficulty data.
    /// Manages difficulty state coordination and UI updates.
    /// The actual provider logic is now in separate focused provider classes.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyDataController : IUITemplateControllerData, ITickable
    {
        private readonly UITemplateDifficultyData  difficultyData;
        private readonly SignalBus                 signalBus;
        private readonly IDynamicDifficultyService difficultyService;
        private readonly IHandleUserDataServices   handleUserDataServices;
        private readonly ILogger                   logger;

        // Track time for ITickable
        private float sessionTime;

        [Preserve]
        public UITemplateDifficultyDataController(
            UITemplateDifficultyData difficultyData,
            SignalBus signalBus,
            IHandleUserDataServices handleUserDataServices,
            IDynamicDifficultyService difficultyService,
            ILogger logger)
        {
            this.difficultyData        = difficultyData;
            this.signalBus             = signalBus;
            this.handleUserDataServices = handleUserDataServices;
            this.difficultyService     = difficultyService;
            this.logger                = logger;

            // Initialize difficulty for new players
            if (this.difficultyData.CurrentDifficulty <= 0)
            {
                var defaultDifficulty = this.difficultyService.GetDefaultDifficulty();
                this.difficultyData.CurrentDifficulty = defaultDifficulty;
                this.logger?.Info($"[UITemplateDifficultyController] New player initialized with default difficulty: {defaultDifficulty:F1}");
            }

            // Subscribe to application lifecycle events
            this.signalBus.Subscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus.Subscribe<ApplicationPauseSignal>(this.OnApplicationPause);

            // Subscribe to game signals for difficulty updates
            this.signalBus.Subscribe<LevelStartedSignal>(this.OnLevelStarted);
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);

            this.logger?.Info($"[UITemplateDifficultyController] Controller initialized. Current difficulty: {this.difficultyData.CurrentDifficulty:F1}");
        }

        #region Public Properties

        /// <summary>
        /// Gets the current difficulty value.
        /// </summary>
        public float CurrentDifficulty => this.difficultyData.CurrentDifficulty;

        /// <summary>
        /// Gets the session time in seconds.
        /// </summary>
        public float SessionTime => this.sessionTime;

        #endregion

        #region ITickable Implementation

        public void Tick()
        {
            this.sessionTime += UnityEngine.Time.deltaTime;
        }

        #endregion

        #region Signal Handlers

        private void OnLevelStarted(LevelStartedSignal signal)
        {
            // Log level start with current difficulty
            this.logger?.Info($"[UITemplateDifficultyController] Level {signal.Level} started with difficulty: {this.CurrentDifficulty:F1}");
        }

        private void OnLevelEnded(LevelEndedSignal signal)
        {
            // Recalculate difficulty after level ends
            // Create session data for the calculation
            var sessionData = new PlayerSessionData
            {
                WinStreak = 0,  // Will be provided by providers
                LossStreak = 0, // Will be provided by providers
                LastPlayTime = DateTime.UtcNow,
            };

            var result = this.difficultyService.CalculateDifficulty(this.difficultyData.CurrentDifficulty, sessionData);

            if (result != null && Math.Abs(result.NewDifficulty - this.difficultyData.CurrentDifficulty) > 0.01f)
            {
                var oldDifficulty = this.difficultyData.CurrentDifficulty;
                this.difficultyData.CurrentDifficulty = result.NewDifficulty;

                // Save the updated difficulty
                this.handleUserDataServices.SaveAll();

                this.logger?.Info($"[UITemplateDifficultyController] Difficulty adjusted: {oldDifficulty:F1} -> {result.NewDifficulty:F1} " +
                    $"(Change: {result.TotalAdjustment:+0.##;-0.##})");

                // Fire a signal for UI updates if needed
                this.signalBus.Fire(new DifficultyChangedSignal(oldDifficulty, result.NewDifficulty));
            }

            var levelResult = signal.IsWin ? "Won" : "Lost";
            this.logger?.Info($"[UITemplateDifficultyController] Level {signal.Level} {levelResult}. Current difficulty: {this.CurrentDifficulty:F1}");
        }

        private void OnApplicationQuit(ApplicationQuitSignal signal)
        {
            // Save difficulty data on quit
            this.handleUserDataServices.SaveAll();
            this.logger?.Info($"[UITemplateDifficultyController] Application quit. Final difficulty: {this.CurrentDifficulty:F1}");
        }

        private void OnApplicationPause(ApplicationPauseSignal signal)
        {
            if (signal.PauseStatus)
            {
                // Save data when app goes to background
                this.handleUserDataServices.SaveAll();
                this.logger?.Info($"[UITemplateDifficultyController] Application paused. Difficulty saved: {this.CurrentDifficulty:F1}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually triggers difficulty recalculation.
        /// </summary>
        /// <summary>
        /// Manually triggers difficulty recalculation.
        /// </summary>
        public void RecalculateDifficulty()
        {
            // Create session data for the calculation
            var sessionData = new PlayerSessionData
            {
                WinStreak = 0,  // Will be provided by providers
                LossStreak = 0, // Will be provided by providers
                LastPlayTime = DateTime.UtcNow,
            };

            var result = this.difficultyService.CalculateDifficulty(this.difficultyData.CurrentDifficulty, sessionData);

            if (result != null && Math.Abs(result.NewDifficulty - this.difficultyData.CurrentDifficulty) > 0.01f)
            {
                var oldDifficulty = this.difficultyData.CurrentDifficulty;
                this.difficultyData.CurrentDifficulty = result.NewDifficulty;

                // Save the updated difficulty
                this.handleUserDataServices.SaveAll();

                this.logger?.Info($"[UITemplateDifficultyController] Manual recalculation: {oldDifficulty:F1} -> {result.NewDifficulty:F1}");

                // Fire a signal for UI updates
                this.signalBus.Fire(new DifficultyChangedSignal(oldDifficulty, result.NewDifficulty));
            }
        }

        /// <summary>
        /// Gets a preview of what the difficulty would be with current conditions.
        /// </summary>
        /// <summary>
        /// Gets a preview of what the difficulty would be with current conditions.
        /// </summary>
        public float PreviewDifficulty()
        {
            // Create session data for the calculation
            var sessionData = new PlayerSessionData
            {
                WinStreak = 0,  // Will be provided by providers
                LossStreak = 0, // Will be provided by providers
                LastPlayTime = DateTime.UtcNow,
            };

            var result = this.difficultyService.CalculateDifficulty(this.difficultyData.CurrentDifficulty, sessionData);
            return result?.NewDifficulty ?? this.CurrentDifficulty;
        }

        /// <summary>
        /// Resets difficulty to default value.
        /// </summary>
        public void ResetDifficulty()
        {
            var defaultDifficulty = this.difficultyService.GetDefaultDifficulty();
            var oldDifficulty = this.difficultyData.CurrentDifficulty;

            this.difficultyData.CurrentDifficulty = defaultDifficulty;
            this.handleUserDataServices.SaveAll();

            this.logger?.Info($"[UITemplateDifficultyController] Difficulty reset: {oldDifficulty:F1} -> {defaultDifficulty:F1}");

            // Fire a signal for UI updates
            this.signalBus.Fire(new DifficultyChangedSignal(oldDifficulty, defaultDifficulty));
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Unsubscribe from signals
            this.signalBus.Unsubscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus.Unsubscribe<ApplicationPauseSignal>(this.OnApplicationPause);
            this.signalBus.Unsubscribe<LevelStartedSignal>(this.OnLevelStarted);
            this.signalBus.Unsubscribe<LevelEndedSignal>(this.OnLevelEnded);

            this.logger?.Info("[UITemplateDifficultyController] Controller disposed");
        }

        #endregion
    }

    /// <summary>
    /// Signal fired when difficulty changes.
    /// </summary>
    public sealed class DifficultyChangedSignal
    {
        public float OldDifficulty { get; }
        public float NewDifficulty { get; }

        public DifficultyChangedSignal(float oldDifficulty, float newDifficulty)
        {
            this.OldDifficulty = oldDifficulty;
            this.NewDifficulty = newDifficulty;
        }
    }
}