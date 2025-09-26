namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.UserData;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
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
        private readonly UITemplateDifficultyData  difficultyData;
        private readonly SignalBus                 signalBus;
        private readonly IDynamicDifficultyService difficultyService;
        private readonly IHandleUserDataServices   handleUserDataServices;
        private readonly UITemplateLevelDataController levelController;
        private readonly ILogger                   logger;

        [Preserve]
        public UITemplateDifficultyDataController(
            UITemplateDifficultyData difficultyData,
            SignalBus signalBus,
            IHandleUserDataServices handleUserDataServices,
            IDynamicDifficultyService difficultyService,
            UITemplateLevelDataController levelController,
            ILogger logger)
        {
            this.difficultyData        = difficultyData;
            this.signalBus             = signalBus;
            this.handleUserDataServices = handleUserDataServices;
            this.difficultyService     = difficultyService;
            this.levelController       = levelController;
            this.logger                = logger;

            // Initialize difficulty for new players
            if (this.difficultyData.CurrentDifficulty <= 0)
            {
                var defaultDifficulty = this.difficultyService.GetDefaultDifficulty();
                this.difficultyData.CurrentDifficulty = defaultDifficulty;
                this.logger?.Info($"[UITemplateDifficultyController] New player initialized with default difficulty: {defaultDifficulty:F1}");
            }

            // Subscribe to level completion for automatic difficulty recalculation
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);

            this.logger?.Info($"[UITemplateDifficultyController] Initialized. Current difficulty: {this.difficultyData.CurrentDifficulty:F1}");
        }

        #region Public Properties

        /// <summary>
        /// Gets the current difficulty value (1-10 scale).
        /// Games should use this value to adjust their parameters accordingly.
        /// </summary>
        public float CurrentDifficulty => this.difficultyData.CurrentDifficulty;

        #endregion

        #region Signal Handlers

        private void OnLevelEnded(LevelEndedSignal signal)
        {
            // The difficulty service will use the providers to get all necessary data
            // We just need to trigger the calculation with the current difficulty
            var result = this.difficultyService.CalculateDifficulty(this.difficultyData.CurrentDifficulty, null);

            if (result != null && Math.Abs(result.NewDifficulty - this.difficultyData.CurrentDifficulty) > DifficultyConstants.EPSILON)
            {
                var oldDifficulty = this.difficultyData.CurrentDifficulty;
                this.difficultyData.CurrentDifficulty = result.NewDifficulty;

                // Also update the level's dynamic difficulty for future reference
                var levelData = this.levelController.GetLevelData(signal.Level);
                levelData.DynamicDifficulty = result.NewDifficulty;

                // Save the updated difficulty
                this.handleUserDataServices.SaveAll();

                this.logger?.Info($"[UITemplateDifficultyController] Difficulty adjusted: {oldDifficulty:F1} -> {result.NewDifficulty:F1} " +
                    $"(Change: {result.TotalAdjustment:+0.##;-0.##})");

                // Fire a signal for UI updates if needed
                this.signalBus.Fire(new DifficultyChangedSignal(oldDifficulty, result.NewDifficulty));
            }
            else if (result != null)
            {
                // Even if difficulty didn't change significantly, update level's dynamic difficulty
                var levelData = this.levelController.GetLevelData(signal.Level);
                levelData.DynamicDifficulty = this.difficultyData.CurrentDifficulty;
            }

            var levelResult = signal.IsWin ? "Won" : "Lost";
            this.logger?.Info($"[UITemplateDifficultyController] Level {signal.Level} {levelResult}. Current difficulty: {this.CurrentDifficulty:F1}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually triggers difficulty recalculation.
        /// The difficulty service will use the registered providers to get all necessary data.
        /// </summary>
        public void RecalculateDifficulty()
        {
            var result = this.difficultyService.CalculateDifficulty(this.difficultyData.CurrentDifficulty, null);

            if (result != null && Math.Abs(result.NewDifficulty - this.difficultyData.CurrentDifficulty) > DifficultyConstants.EPSILON)
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
        /// Does not save the change.
        /// </summary>
        public float PreviewDifficulty()
        {
            var result = this.difficultyService.CalculateDifficulty(this.difficultyData.CurrentDifficulty, null);
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