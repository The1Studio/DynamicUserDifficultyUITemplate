namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using global::UITemplate.Scripts.Enum;

    [TestFixture]
    public class UITemplateLevelProgressProviderTests
    {
        private MockLevelDataController mockController;
        private UITemplateLevelProgressProvider provider;

        [SetUp]
        public void Setup()
        {
            this.mockController = new MockLevelDataController();
            this.provider = new UITemplateLevelProgressProvider(this.mockController, null);
        }

        [Test]
        public void GetCurrentLevel_ReturnsCorrectValue()
        {
            // Arrange
            this.mockController.CurrentLevel = 42;

            // Act
            var result = this.provider.GetCurrentLevel();

            // Assert
            Assert.AreEqual(42, result);
        }

        [Test]
        public void GetCurrentLevel_ReturnsOne_WhenControllerIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateLevelProgressProvider(null, null);

            // Act
            var result = nullProvider.GetCurrentLevel();

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetAttemptsOnCurrentLevel_CalculatesCorrectly()
        {
            // Arrange
            this.mockController.CurrentLevel = 5;
            this.mockController.TestLevelData = new LevelData
            {
                Level = 5,
                WinCount = 3,
                LoseCount = 7
            };

            // Act
            var result = this.provider.GetAttemptsOnCurrentLevel();

            // Assert
            Assert.AreEqual(10, result); // 3 + 7
        }

        [Test]
        public void GetAttemptsOnCurrentLevel_ReturnsZero_WhenNoData()
        {
            // Arrange
            this.mockController.CurrentLevel = 5;
            this.mockController.TestLevelData = null;

            // Act
            var result = this.provider.GetAttemptsOnCurrentLevel();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetCurrentLevelDifficulty_MapsEasyCorrectly()
        {
            // Arrange
            this.mockController.CurrentLevel = 1;
            this.mockController.TestLevelData = new LevelData
            {
                Level = 1,
                StaticDifficulty = LevelDifficulty.Easy
            };

            // Act
            var result = this.provider.GetCurrentLevelDifficulty();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.EASY_DIFFICULTY_VALUE, result);
        }

        [Test]
        public void GetCurrentLevelDifficulty_MapsNormalCorrectly()
        {
            // Arrange
            this.mockController.CurrentLevel = 1;
            this.mockController.TestLevelData = new LevelData
            {
                Level = 1,
                StaticDifficulty = LevelDifficulty.Normal
            };

            // Act
            var result = this.provider.GetCurrentLevelDifficulty();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.NORMAL_DIFFICULTY_VALUE, result);
        }

        [Test]
        public void GetCurrentLevelDifficulty_MapsHardCorrectly()
        {
            // Arrange
            this.mockController.CurrentLevel = 1;
            this.mockController.TestLevelData = new LevelData
            {
                Level = 1,
                StaticDifficulty = LevelDifficulty.Hard
            };

            // Act
            var result = this.provider.GetCurrentLevelDifficulty();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.HARD_DIFFICULTY_VALUE, result);
        }

        [Test]
        public void GetCurrentLevelDifficulty_MapsVeryHardCorrectly()
        {
            // Arrange
            this.mockController.CurrentLevel = 1;
            this.mockController.TestLevelData = new LevelData
            {
                Level = 1,
                StaticDifficulty = LevelDifficulty.VeryHard
            };

            // Act
            var result = this.provider.GetCurrentLevelDifficulty();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.VERY_HARD_DIFFICULTY_VALUE, result);
        }

        [Test]
        public void GetCurrentLevelDifficulty_ReturnsDefault_WhenNoData()
        {
            // Arrange
            this.mockController.CurrentLevel = 1;
            this.mockController.TestLevelData = null;

            // Act
            var result = this.provider.GetCurrentLevelDifficulty();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.DEFAULT_DIFFICULTY, result);
        }

        [Test]
        public void GetAverageCompletionTime_CalculatesCorrectly()
        {
            // Arrange
            this.mockController.TestLevels = new List<LevelData>
            {
                new LevelData { LevelStatus = LevelData.Status.Passed, WinTime = 30 },
                new LevelData { LevelStatus = LevelData.Status.Passed, WinTime = 40 },
                new LevelData { LevelStatus = LevelData.Status.Passed, WinTime = 50 },
                new LevelData { LevelStatus = LevelData.Status.NotPassed, WinTime = 0 } // Should be ignored
            };

            // Act
            var result = this.provider.GetAverageCompletionTime();

            // Assert
            Assert.AreEqual(40f, result); // (30 + 40 + 50) / 3
        }

        [Test]
        public void GetAverageCompletionTime_ReturnsDefault_WhenNoCompletedLevels()
        {
            // Arrange
            this.mockController.TestLevels = new List<LevelData>
            {
                new LevelData { LevelStatus = LevelData.Status.NotPassed, WinTime = 0 }
            };

            // Act
            var result = this.provider.GetAverageCompletionTime();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.DEFAULT_COMPLETION_TIME_SECONDS, result);
        }

        [Test]
        public void GetCompletionRate_CalculatesCorrectly()
        {
            // Arrange
            this.mockController.TestLevels = new List<LevelData>
            {
                new LevelData { WinCount = 3, LoseCount = 2 },
                new LevelData { WinCount = 5, LoseCount = 5 },
                new LevelData { WinCount = 2, LoseCount = 3 }
            };

            // Act
            var result = this.provider.GetCompletionRate();

            // Assert
            Assert.AreEqual(0.5f, result); // 10 wins / 20 total attempts
        }

        [Test]
        public void GetCompletionRate_ReturnsZero_WhenNoAttempts()
        {
            // Arrange
            this.mockController.TestLevels = new List<LevelData>
            {
                new LevelData { WinCount = 0, LoseCount = 0 }
            };

            // Act
            var result = this.provider.GetCompletionRate();

            // Assert
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenControllerIsNull()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                new UITemplateLevelProgressProvider(null, null));
        }

        // Mock controller for testing
        private class MockLevelDataController : UITemplateLevelDataController
        {
            public override int CurrentLevel { get; set; }
            public LevelData TestLevelData { get; set; }
            public List<LevelData> TestLevels { get; set; }

            public MockLevelDataController() : base(null, null, null, null) { }

            public override LevelData GetLevelData(int level)
            {
                return this.TestLevelData;
            }

            public override List<LevelData> GetAllLevels()
            {
                return this.TestLevels ?? new List<LevelData>();
            }
        }
    }
}