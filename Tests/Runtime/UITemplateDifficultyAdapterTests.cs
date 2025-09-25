namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Tests
{
    using System;
    using GameFoundation.Signals;
    using NUnit.Framework;
    using TheOne.Logging;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Signals;

    [TestFixture]
    public class UITemplateDifficultyAdapterTests
    {
        private MockSignalBus mockSignalBus;
        private MockDifficultyController mockController;
        private UITemplateDifficultyAdapter adapter;

        [SetUp]
        public void Setup()
        {
            this.mockSignalBus = new MockSignalBus();
            this.mockController = new MockDifficultyController();
            this.adapter = new UITemplateDifficultyAdapter(this.mockSignalBus, this.mockController, null);
        }

        [Test]
        public void CurrentDifficulty_ReturnsControllerValue()
        {
            // Arrange
            this.mockController.TestDifficulty = 7.5f;

            // Act
            var result = this.adapter.CurrentDifficulty;

            // Assert
            Assert.AreEqual(7.5f, result);
        }

        [Test]
        public void Initialize_SubscribesToSignals()
        {
            // Act
            this.adapter.Initialize();

            // Assert
            Assert.AreEqual(5, this.mockSignalBus.SubscriptionCount);
            Assert.IsTrue(this.mockSignalBus.HasSubscription<LevelStartedSignal>());
            Assert.IsTrue(this.mockSignalBus.HasSubscription<LevelEndedSignal>());
            Assert.IsTrue(this.mockSignalBus.HasSubscription<LevelSkippedSignal>());
            Assert.IsTrue(this.mockSignalBus.HasSubscription<ApplicationQuitSignal>());
            Assert.IsTrue(this.mockSignalBus.HasSubscription<ApplicationPauseSignal>());
        }

        [Test]
        public void Dispose_UnsubscribesFromSignals()
        {
            // Arrange
            this.adapter.Initialize();

            // Act
            this.adapter.Dispose();

            // Assert
            Assert.AreEqual(5, this.mockSignalBus.UnsubscriptionCount);
        }

        [Test]
        public void GetAdjustedParameters_ReturnsCorrectValuesForEasyDifficulty()
        {
            // Arrange
            this.mockController.TestDifficulty = 2f;

            // Act
            var parameters = this.adapter.GetAdjustedParameters();

            // Assert
            Assert.GreaterOrEqual(parameters.ScrewSpeed, 0.8f);
            Assert.LessOrEqual(parameters.ScrewSpeed, 1.0f);
            Assert.GreaterOrEqual(parameters.TimeLimit, 120f);
            Assert.LessOrEqual(parameters.TimeLimit, 180f);
            Assert.GreaterOrEqual(parameters.TargetScore, 100);
            Assert.LessOrEqual(parameters.TargetScore, 300);
            Assert.GreaterOrEqual(parameters.HintDelay, 5f);
            Assert.LessOrEqual(parameters.HintDelay, 10f);
            Assert.GreaterOrEqual(parameters.BoosterEffectiveness, 1.2f);
            Assert.LessOrEqual(parameters.BoosterEffectiveness, 1.5f);
        }

        [Test]
        public void GetAdjustedParameters_ReturnsCorrectValuesForNormalDifficulty()
        {
            // Arrange
            this.mockController.TestDifficulty = 5f;

            // Act
            var parameters = this.adapter.GetAdjustedParameters();

            // Assert
            Assert.GreaterOrEqual(parameters.ScrewSpeed, 1.0f);
            Assert.LessOrEqual(parameters.ScrewSpeed, 1.2f);
            Assert.GreaterOrEqual(parameters.TimeLimit, 90f);
            Assert.LessOrEqual(parameters.TimeLimit, 120f);
            Assert.GreaterOrEqual(parameters.TargetScore, 300);
            Assert.LessOrEqual(parameters.TargetScore, 600);
            Assert.GreaterOrEqual(parameters.HintDelay, 10f);
            Assert.LessOrEqual(parameters.HintDelay, 20f);
            Assert.GreaterOrEqual(parameters.BoosterEffectiveness, 1.0f);
            Assert.LessOrEqual(parameters.BoosterEffectiveness, 1.2f);
        }

        [Test]
        public void GetAdjustedParameters_ReturnsCorrectValuesForHardDifficulty()
        {
            // Arrange
            this.mockController.TestDifficulty = 9f;

            // Act
            var parameters = this.adapter.GetAdjustedParameters();

            // Assert
            Assert.GreaterOrEqual(parameters.ScrewSpeed, 1.2f);
            Assert.LessOrEqual(parameters.ScrewSpeed, 1.5f);
            Assert.GreaterOrEqual(parameters.TimeLimit, 60f);
            Assert.LessOrEqual(parameters.TimeLimit, 90f);
            Assert.GreaterOrEqual(parameters.TargetScore, 600);
            Assert.LessOrEqual(parameters.TargetScore, 1000);
            Assert.GreaterOrEqual(parameters.HintDelay, 20f);
            Assert.LessOrEqual(parameters.HintDelay, 30f);
            Assert.GreaterOrEqual(parameters.BoosterEffectiveness, 0.8f);
            Assert.LessOrEqual(parameters.BoosterEffectiveness, 1.0f);
        }

        [Test]
        public void GameParameters_DefaultValues_UseConstants()
        {
            // Arrange & Act
            var parameters = new GameParameters();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.SCREW_SPEED_NORMAL_MIN, parameters.ScrewSpeed);
            Assert.AreEqual(UITemplateIntegrationConstants.TIME_LIMIT_NORMAL_MAX, parameters.TimeLimit);
            Assert.AreEqual(UITemplateIntegrationConstants.TARGET_SCORE_NORMAL_MAX, parameters.TargetScore);
            Assert.AreEqual(UITemplateIntegrationConstants.HINT_DELAY_NORMAL_MIN, parameters.HintDelay);
            Assert.AreEqual(UITemplateIntegrationConstants.BOOSTER_EFFECT_NORMAL_MIN, parameters.BoosterEffectiveness);
        }

        [Test]
        public void GetAdjustedParameters_HandlesEdgeCases()
        {
            // Test minimum difficulty
            this.mockController.TestDifficulty = UITemplateIntegrationConstants.MIN_DIFFICULTY;
            var minParams = this.adapter.GetAdjustedParameters();
            Assert.IsNotNull(minParams);

            // Test maximum difficulty
            this.mockController.TestDifficulty = UITemplateIntegrationConstants.MAX_DIFFICULTY;
            var maxParams = this.adapter.GetAdjustedParameters();
            Assert.IsNotNull(maxParams);

            // Test exact threshold values
            this.mockController.TestDifficulty = UITemplateIntegrationConstants.EASY_DIFFICULTY_THRESHOLD;
            var easyThresholdParams = this.adapter.GetAdjustedParameters();
            Assert.IsNotNull(easyThresholdParams);

            this.mockController.TestDifficulty = UITemplateIntegrationConstants.NORMAL_DIFFICULTY_THRESHOLD;
            var normalThresholdParams = this.adapter.GetAdjustedParameters();
            Assert.IsNotNull(normalThresholdParams);
        }

        // Mock implementations for testing
        private class MockSignalBus : SignalBus
        {
            public int SubscriptionCount { get; private set; }
            public int UnsubscriptionCount { get; private set; }
            private readonly System.Collections.Generic.HashSet<Type> subscribedTypes = new();

            public override void Subscribe<T>(System.Action<T> callback, int order = 0)
            {
                this.SubscriptionCount++;
                this.subscribedTypes.Add(typeof(T));
            }

            public override void Unsubscribe<T>(System.Action<T> callback)
            {
                this.UnsubscriptionCount++;
            }

            public bool HasSubscription<T>()
            {
                return this.subscribedTypes.Contains(typeof(T));
            }
        }

        private class MockDifficultyController : UITemplateDifficultyDataController
        {
            public float TestDifficulty { get; set; } = 5f;

            public MockDifficultyController() : base(null, null, null) { }

            public override float CurrentDifficulty => this.TestDifficulty;
        }
    }
}