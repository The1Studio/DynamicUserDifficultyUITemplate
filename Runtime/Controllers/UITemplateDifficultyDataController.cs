namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers
{
    using System;
    using GameFoundation.Scripts.Utilities.UserData;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Signals;
    using UnityEngine.Scripting;

    /// <summary>
    /// UITemplate controller for Dynamic User Difficulty.
    /// Provides access to the current difficulty value and handles automatic recalculation on level completion.
    /// The difficulty calculation is handled by the IDynamicDifficultyService using data from providers.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyDataController : IUITemplateControllerData
    {
        private readonly SignalBus                 signalBus;
        private readonly IDynamicDifficultyService difficultyService;
        private readonly IHandleUserDataServices   handleUserDataServices;
        private readonly UITemplateLevelDataController levelController;
        private readonly ILogger                   logger;

        [Preserve]
        public UITemplateDifficultyDataController(
            SignalBus signalBus,
            IHandleUserDataServices handleUserDataServices,
            IDynamicDifficultyService difficultyService,
            UITemplateLevelDataController levelController,
            ILogger logger)
        {
            this.signalBus             = signalBus;
            this.handleUserDataServices = handleUserDataServices;
            this.difficultyService     = difficultyService;
            this.levelController       = levelController;
            this.logger                = logger;

            // Initialize difficulty for new players
            if (this.difficultyService.CurrentDifficulty <= 0)
            {
                var defaultDifficulty = this.difficultyService.GetDefaultDifficulty();
                var result = new DifficultyResult
                {
                    NewDifficulty = defaultDifficulty,
                    PreviousDifficulty = 0
                };
                this.difficultyService.ApplyDifficulty(result);
                this.logger?.Info($"[UITemplateDifficultyController] New player initialized with default difficulty: {defaultDifficulty:F1}");
            }

            // Subscribe to level completion for automatic difficulty recalculation
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);

            this.logger?.Info($"[UITemplateDifficultyController] Initialized. Current difficulty: {this.difficultyService.CurrentDifficulty:F1}");
        }

        #region Public Properties

        /// <summary>
        /// Gets the current difficulty value (1-10 scale).
        /// Games should use this value to adjust their parameters accordingly.
        /// </summary>
        public float CurrentDifficulty => this.difficultyService.CurrentDifficulty;

        #endregion

        #region Signal Handlers

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

                this.logger?.Info($"[UITemplateDifficultyController] Difficulty adjusted: {result.PreviousDifficulty:F1} -> {result.NewDifficulty:F1} " +
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
            this.logger?.Info($"[UITemplateDifficultyController] Level {signal.Level} {levelResult}. Current difficulty: {this.difficultyService.CurrentDifficulty:F1}");
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Unsubscribe from signals
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