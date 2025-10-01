namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    using System;
    using GameFoundation.Scripts.Utilities.UserData;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Signals;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Signals;
    using UnityEngine.Scripting;

    /// <summary>
    /// Adapter that handles all difficulty calculation logic and lifecycle events.
    /// This class contains the business logic for difficulty management, while
    /// UITemplateDifficultyDataController only exposes data access.
    /// </summary>
    [Preserve]
    public class DifficultyAdapter : IDisposable
    {
        private readonly SignalBus                     signalBus;
        private readonly IDynamicDifficultyService     difficultyService;
        private readonly IHandleUserDataServices       handleUserDataServices;
        private readonly UITemplateLevelDataController levelController;
        private readonly ILogger                       logger;

        [Preserve]
        public DifficultyAdapter(
            SignalBus signalBus,
            IDynamicDifficultyService difficultyService,
            IHandleUserDataServices handleUserDataServices,
            UITemplateLevelDataController levelController,
            ILogger logger)
        {
            this.signalBus              = signalBus;
            this.difficultyService      = difficultyService;
            this.handleUserDataServices = handleUserDataServices;
            this.levelController        = levelController;
            this.logger                 = logger;

            // Initialize difficulty for new players
            this.InitializeNewPlayer();

            // Subscribe to level completion for automatic difficulty recalculation
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);

            this.logger?.Info($"[DifficultyAdapter] Initialized. Current difficulty: {this.difficultyService.CurrentDifficulty:F1}");
        }

        #region Initialization

        /// <summary>
        /// Initializes difficulty for new players who have never played before.
        /// </summary>
        private void InitializeNewPlayer()
        {
            if (this.difficultyService.CurrentDifficulty <= 0)
            {
                var defaultDifficulty = this.difficultyService.GetDefaultDifficulty();
                var result = new DifficultyResult
                {
                    NewDifficulty      = defaultDifficulty,
                    PreviousDifficulty = 0
                };
                this.difficultyService.ApplyDifficulty(result);
                this.logger?.Info($"[DifficultyAdapter] New player initialized with default difficulty: {defaultDifficulty:F1}");
            }
        }

        #endregion

        #region Signal Handlers

        /// <summary>
        /// Handles level completion events and recalculates difficulty based on player performance.
        /// </summary>
        private void OnLevelEnded(LevelEndedSignal signal)
        {
            // The difficulty service will use the providers to get all necessary data
            // We just need to trigger the calculation
            var result = this.difficultyService.CalculateDifficulty();

            if (result != null && Math.Abs(result.NewDifficulty - result.PreviousDifficulty) > DifficultyConstants.EPSILON)
            {
                // Apply the difficulty through the service
                this.difficultyService.ApplyDifficulty(result);

                // Also update the level's dynamic difficulty for future reference
                this.levelController.UpdateLevelDifficulty(signal.Level, signal.Mode, result.NewDifficulty);

                // Save the updated difficulty
                this.handleUserDataServices.SaveAll();

                this.logger?.Info($"[DifficultyAdapter] Difficulty adjusted: {result.PreviousDifficulty:F1} -> {result.NewDifficulty:F1} " +
                                  $"(Change: {result.TotalAdjustment:+0.##;-0.##})");

                // Fire a signal for UI updates if needed
                this.signalBus.Fire(new DifficultyChangedSignal(result.PreviousDifficulty, result.NewDifficulty));
            }
            else if (result != null)
            {
                // Even if difficulty didn't change significantly, update level's dynamic difficulty
                this.levelController.UpdateLevelDifficulty(signal.Level, signal.Mode, this.difficultyService.CurrentDifficulty);
            }

            var levelResult = signal.IsWin ? "Won" : "Lost";
            this.logger?.Info($"[DifficultyAdapter] Level {signal.Level} {levelResult}. Current difficulty: {this.difficultyService.CurrentDifficulty:F1}");
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Unsubscribe from signals to prevent memory leaks
            this.signalBus.Unsubscribe<LevelEndedSignal>(this.OnLevelEnded);

            this.logger?.Info("[DifficultyAdapter] Adapter disposed");
        }

        #endregion
    }
}