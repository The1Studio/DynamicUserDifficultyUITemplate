namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Scripts.Utilities.ApplicationServices;
    using GameFoundation.Signals;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Signals;
    using UnityEngine.Scripting;

    /// <summary>
    /// Adapter that connects UITemplate's signal system to Dynamic Difficulty calculations.
    /// Listens to game events and logs them for the difficulty system.
    /// The actual difficulty calculation is handled by the controller and service.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyAdapter : IInitializable, IDisposable
    {
        private readonly SignalBus signalBus;
        private readonly UITemplateDifficultyDataController difficultyController;
        private readonly ILogger logger;

        private DateTime levelStartTime;

        [Preserve]
        public UITemplateDifficultyAdapter(
            SignalBus signalBus,
            UITemplateDifficultyDataController difficultyController,
            ILogger logger)
        {
            this.signalBus = signalBus;
            this.difficultyController = difficultyController;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the current difficulty value.
        /// </summary>
        public float CurrentDifficulty => this.difficultyController.CurrentDifficulty;

        public void Initialize()
        {
            // Subscribe to UITemplate signals
            this.signalBus.Subscribe<LevelStartedSignal>(this.OnLevelStarted);
            this.signalBus.Subscribe<LevelEndedSignal>(this.OnLevelEnded);
            this.signalBus.Subscribe<LevelSkippedSignal>(this.OnLevelSkipped);
            this.signalBus.Subscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus.Subscribe<ApplicationPauseSignal>(this.OnApplicationPause);

            this.logger?.Info($"[UITemplateDifficultyAdapter] Initialized. Current difficulty: {this.CurrentDifficulty:F1}");
        }

        private void OnLevelStarted(LevelStartedSignal signal)
        {
            this.levelStartTime = DateTime.UtcNow;
            this.logger?.Info($"[UITemplateDifficultyAdapter] Level {signal.Level} started, Difficulty: {this.CurrentDifficulty:F1}");
        }

        private void OnLevelEnded(LevelEndedSignal signal)
        {
            var completionTime = (float)(DateTime.UtcNow - this.levelStartTime).TotalSeconds;

            // The controller will handle difficulty recalculation automatically via its signal handlers
            // We just need to log the event here
            var result = signal.IsWin ? "won" : "lost";
            this.logger?.Info($"[UITemplateDifficultyAdapter] Level {signal.Level} {result} in {completionTime:F1}s");

            // Check current difficulty after the level end signal is processed
            this.logger?.Info($"[UITemplateDifficultyAdapter] Current difficulty: {this.CurrentDifficulty:F1}");
        }

        private void OnLevelSkipped(LevelSkippedSignal signal)
        {
            var completionTime = (float)(DateTime.UtcNow - this.levelStartTime).TotalSeconds;
            this.logger?.Info($"[UITemplateDifficultyAdapter] Level {signal.Level} skipped after {completionTime:F1}s - treated as loss");
        }

        private void OnApplicationQuit(ApplicationQuitSignal signal)
        {
            var sessionDuration = (float)(DateTime.UtcNow - this.levelStartTime).TotalSeconds;
            this.logger?.Info($"[UITemplateDifficultyAdapter] Application quit - Session duration: {sessionDuration:F1}s");
        }

        private void OnApplicationPause(ApplicationPauseSignal signal)
        {
            if (signal.PauseStatus)
            {
                // App going to background
                this.logger?.Info($"[UITemplateDifficultyAdapter] Application paused");
            }
            else
            {
                // App returning from background
                this.logger?.Info($"[UITemplateDifficultyAdapter] Application resumed");

                // The controller will handle recalculation if needed
            }
        }

        /// <summary>
        /// Gets adjusted game parameters based on current difficulty.
        /// </summary>
        public GameParameters GetAdjustedParameters()
        {
            // Map difficulty to game parameters
            var difficulty = this.CurrentDifficulty;

            return new GameParameters
            {
                ScrewSpeed           = this.MapScrewSpeed(difficulty),
                TimeLimit            = this.MapTimeLimit(difficulty),
                TargetScore          = this.MapTargetScore(difficulty),
                HintDelay            = this.MapHintDelay(difficulty),
                BoosterEffectiveness = this.MapBoosterEffectiveness(difficulty),
            };
        }

        private float MapScrewSpeed(float difficulty)
        {
            // Lower difficulty = slower screws
            // 1-3: Slow (0.8x to 1.0x)
            // 4-7: Normal (1.0x to 1.2x)
            // 8-10: Fast (1.2x to 1.5x)
            if (difficulty <= UITemplateIntegrationConstants.EASY_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.SCREW_SPEED_SLOW_MIN + (difficulty - UITemplateIntegrationConstants.MIN_DIFFICULTY) * UITemplateIntegrationConstants.EASY_SCREW_SPEED_FACTOR;
            else if (difficulty <= UITemplateIntegrationConstants.NORMAL_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.SCREW_SPEED_NORMAL_MIN + (difficulty - 4f) * UITemplateIntegrationConstants.NORMAL_SCREW_SPEED_FACTOR;
            else
                return UITemplateIntegrationConstants.SCREW_SPEED_FAST_MIN + (difficulty - 8f) * UITemplateIntegrationConstants.HARD_SCREW_SPEED_FACTOR;
        }

        private float MapTimeLimit(float difficulty)
        {
            // Lower difficulty = more time
            // 1-3: 180-120 seconds
            // 4-7: 120-90 seconds
            // 8-10: 90-60 seconds
            if (difficulty <= UITemplateIntegrationConstants.EASY_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.TIME_LIMIT_EASY_MAX - (difficulty - UITemplateIntegrationConstants.MIN_DIFFICULTY) * UITemplateIntegrationConstants.EASY_TIME_REDUCTION_FACTOR;
            else if (difficulty <= UITemplateIntegrationConstants.NORMAL_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.TIME_LIMIT_NORMAL_MAX - (difficulty - 4f) * UITemplateIntegrationConstants.NORMAL_TIME_REDUCTION_FACTOR;
            else
                return UITemplateIntegrationConstants.TIME_LIMIT_HARD_MAX - (difficulty - 8f) * UITemplateIntegrationConstants.HARD_TIME_REDUCTION_FACTOR;
        }

        private int MapTargetScore(float difficulty)
        {
            // Higher difficulty = higher target score
            // 1-3: 100-300
            // 4-7: 300-600
            // 8-10: 600-1000
            if (difficulty <= UITemplateIntegrationConstants.EASY_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.TARGET_SCORE_EASY_MIN + (int)((difficulty - UITemplateIntegrationConstants.MIN_DIFFICULTY) * UITemplateIntegrationConstants.EASY_SCORE_INCREMENT);
            else if (difficulty <= UITemplateIntegrationConstants.NORMAL_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.TARGET_SCORE_NORMAL_MIN + (int)((difficulty - 4f) * UITemplateIntegrationConstants.NORMAL_SCORE_INCREMENT);
            else
                return UITemplateIntegrationConstants.TARGET_SCORE_HARD_MIN + (int)((difficulty - 8f) * UITemplateIntegrationConstants.HARD_SCORE_INCREMENT);
        }

        private float MapHintDelay(float difficulty)
        {
            // Higher difficulty = longer hint delay
            // 1-3: 5-10 seconds
            // 4-7: 10-20 seconds
            // 8-10: 20-30 seconds
            if (difficulty <= UITemplateIntegrationConstants.EASY_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.HINT_DELAY_EASY_MIN + (difficulty - UITemplateIntegrationConstants.MIN_DIFFICULTY) * UITemplateIntegrationConstants.EASY_HINT_INCREMENT;
            else if (difficulty <= UITemplateIntegrationConstants.NORMAL_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.HINT_DELAY_NORMAL_MIN + (difficulty - 4f) * UITemplateIntegrationConstants.NORMAL_HINT_INCREMENT;
            else
                return UITemplateIntegrationConstants.HINT_DELAY_HARD_MIN + (difficulty - 8f) * UITemplateIntegrationConstants.HARD_HINT_INCREMENT;
        }

        private float MapBoosterEffectiveness(float difficulty)
        {
            // Lower difficulty = more effective boosters
            // 1-3: 1.5x to 1.2x
            // 4-7: 1.2x to 1.0x
            // 8-10: 1.0x to 0.8x
            if (difficulty <= UITemplateIntegrationConstants.EASY_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.BOOSTER_EFFECT_EASY_MAX - (difficulty - UITemplateIntegrationConstants.MIN_DIFFICULTY) * UITemplateIntegrationConstants.EASY_BOOSTER_DECREMENT;
            else if (difficulty <= UITemplateIntegrationConstants.NORMAL_DIFFICULTY_THRESHOLD)
                return UITemplateIntegrationConstants.BOOSTER_EFFECT_NORMAL_MAX - (difficulty - 4f) * UITemplateIntegrationConstants.NORMAL_BOOSTER_DECREMENT;
            else
                return UITemplateIntegrationConstants.BOOSTER_EFFECT_HARD_MAX - (difficulty - 8f) * UITemplateIntegrationConstants.HARD_BOOSTER_DECREMENT;
        }

        public void Dispose()
        {
            // Unsubscribe from signals
            this.signalBus.Unsubscribe<LevelStartedSignal>(this.OnLevelStarted);
            this.signalBus.Unsubscribe<LevelEndedSignal>(this.OnLevelEnded);
            this.signalBus.Unsubscribe<LevelSkippedSignal>(this.OnLevelSkipped);
            this.signalBus.Unsubscribe<ApplicationQuitSignal>(this.OnApplicationQuit);
            this.signalBus.Unsubscribe<ApplicationPauseSignal>(this.OnApplicationPause);

            this.logger?.Info("[UITemplateDifficultyAdapter] Disposed");
        }
    }

    /// <summary>
    /// Game parameters adjusted by difficulty.
    /// </summary>
    public sealed class GameParameters
    {
        public float ScrewSpeed { get; set; } = UITemplateIntegrationConstants.SCREW_SPEED_NORMAL_MIN;
        public float TimeLimit { get; set; } = UITemplateIntegrationConstants.TIME_LIMIT_NORMAL_MAX;
        public int TargetScore { get; set; } = UITemplateIntegrationConstants.TARGET_SCORE_NORMAL_MAX;
        public float HintDelay { get; set; } = UITemplateIntegrationConstants.HINT_DELAY_NORMAL_MIN;
        public float BoosterEffectiveness { get; set; } = UITemplateIntegrationConstants.BOOSTER_EFFECT_NORMAL_MIN;
    }
}