#nullable enable

namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Signals;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Signals;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Adapter that handles all difficulty calculation logic and lifecycle events.
    /// This class contains the business logic for difficulty management, while
    /// UITemplateDifficultyDataController only exposes data access.
    /// </summary>
    [Preserve]
    public sealed class DifficultyAdapter : IDisposable, IInitializable
    {
        private readonly SignalBus                     signalBus;
        private readonly IDynamicDifficultyService     difficultyService;
        private readonly UITemplateLevelDataController levelController;
        private readonly ILogger                       logger;

        [Preserve]
        public DifficultyAdapter(
            SignalBus                     signalBus,
            IDynamicDifficultyService     difficultyService,
            UITemplateLevelDataController levelController,
            ILogger                       logger
        )
        {
            this.signalBus              = signalBus;
            this.difficultyService      = difficultyService;
            this.levelController        = levelController;
            this.logger                 = logger;
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DifficultyAdapter] Constructed with logger: {this.logger != null}");
            #endif
        }

        #region Initialization


        public void Initialize()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[DifficultyAdapter] ===== INITIALIZING =====");
            #endif

            // Initialize difficulty for new players
            this.InitializeNewPlayer();

            // Subscribe to level completion for automatic difficulty recalculation
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DifficultyAdapter] Initialized. Current difficulty: {this.difficultyService.CurrentDifficulty:F1}");
            #endif
            this.logger?.Info($"[DifficultyAdapter] Initialized. Current difficulty: {this.difficultyService.CurrentDifficulty:F1}");
        }


        /// <summary>
        /// Initializes difficulty for new players who have never played before.
        /// </summary>
        private void InitializeNewPlayer()
        {
            if (this.difficultyService.CurrentDifficulty <= 0)
            {
                var defaultDifficulty = this.difficultyService.GetDefaultDifficulty();
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[DifficultyAdapter] New player - setting default difficulty: {defaultDifficulty:F1}");
                #endif

                var result = new DifficultyResult
                {
                    NewDifficulty      = defaultDifficulty,
                    PreviousDifficulty = 0
                };
                this.difficultyService.ApplyDifficulty(result);

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[DifficultyAdapter] New player initialized with default difficulty: {defaultDifficulty:F1}");
                #endif
                this.logger?.Info($"[DifficultyAdapter] New player initialized with default difficulty: {defaultDifficulty:F1}");
            }
        }

        #endregion

        #region Signal Handlers

        /// <summary>
        /// Handles level completion events and recalculates difficulty based on player performance.
        /// </summary>
        private async void OnLevelEnded(LevelEndedSignal signal)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DifficultyAdapter] ===== ON LEVEL ENDED ===== Level: {signal.Level}, IsWin: {signal.IsWin}");
            #endif

            // CRITICAL FIX: Wait one frame for WinStreakService.OnWon() to execute and update streaks
            // Without this, we read Streak=0 because LoseStreakViaPlay() reset it at level start
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[DifficultyAdapter] ⏳ Waiting one frame for WinStreakService to update streaks...");
            #endif
            await Cysharp.Threading.Tasks.UniTask.NextFrame();

            // The difficulty service will use the providers to get all necessary data
            // We just need to trigger the calculation
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[DifficultyAdapter] ✓ Streak updated. Calling CalculateDifficulty()...");
            #endif
            var result = this.difficultyService.CalculateDifficulty();

            if (result != null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[DifficultyAdapter] Calculation result:");
                #endif
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"  - Previous: {result.PreviousDifficulty:F2}");
                #endif
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"  - New: {result.NewDifficulty:F2}");
                #endif
                Debug.Log($"  - Total Adjustment: {result.TotalAdjustment:+0.##;-0.##}");
                Debug.Log($"  - Change: {result.NewDifficulty - result.PreviousDifficulty:+0.##;-0.##}");

                // Log all modifiers
                if (result.AppliedModifiers != null)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[DifficultyAdapter] Modifiers ({result.AppliedModifiers.Count}):");
                    #endif
                    foreach (var modifier in result.AppliedModifiers)
                    {
                        Debug.Log($"  - {modifier.ModifierName}: {modifier.Value:+0.##;-0.##} ({modifier.Reason})");
                    }
                }
            }
            else
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[DifficultyAdapter] CalculateDifficulty returned NULL!");
                #endif
            }

            if (result != null && Math.Abs(result.NewDifficulty - result.PreviousDifficulty) > DifficultyConstants.EPSILON)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[DifficultyAdapter] Applying difficulty change: {result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");
                #endif

                // Apply the difficulty through the service
                this.difficultyService.ApplyDifficulty(result);

                // Also update the level's dynamic difficulty for future reference
                this.levelController.UpdateLevelDifficulty(signal.Level, signal.Mode, result.NewDifficulty);

                Debug.Log($"[DifficultyAdapter] ✓ Difficulty adjusted: {result.PreviousDifficulty:F1} -> {result.NewDifficulty:F1} (Change: {result.TotalAdjustment:+0.##;-0.##})");
                this.logger?.Info($"[DifficultyAdapter] Difficulty adjusted: {result.PreviousDifficulty:F1} -> {result.NewDifficulty:F1} (Change: {result.TotalAdjustment:+0.##;-0.##})");

                // Fire a signal for UI updates if needed
                this.signalBus.Fire(new DifficultyChangedSignal(result.PreviousDifficulty, result.NewDifficulty));
            }
            else if (result != null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[DifficultyAdapter] No significant change (delta: {Math.Abs(result.NewDifficulty - result.PreviousDifficulty):F4} < {DifficultyConstants.EPSILON:F4})");
                #endif
                // Even if difficulty didn't change significantly, update level's dynamic difficulty
                this.levelController.UpdateLevelDifficulty(signal.Level, signal.Mode, this.difficultyService.CurrentDifficulty);
            }

            var levelResult = signal.IsWin ? "Won" : "Lost";
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[DifficultyAdapter] Level {signal.Level} {levelResult}. Current difficulty: {this.difficultyService.CurrentDifficulty:F2}");
            #endif
            this.logger?.Info($"[DifficultyAdapter] Level {signal.Level} {levelResult}. Current difficulty: {this.difficultyService.CurrentDifficulty:F1}");
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Unsubscribe from signals to prevent memory leaks
            this.signalBus.Unsubscribe<LevelEndedSignal>(this.OnLevelEnded);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[DifficultyAdapter] Adapter disposed");
            #endif
            this.logger?.Info("[DifficultyAdapter] Adapter disposed");
        }

        #endregion
    }
}