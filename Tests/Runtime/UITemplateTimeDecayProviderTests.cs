namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Models;

    [TestFixture]
    public class UITemplateTimeDecayProviderTests
    {
        private MockSessionController mockController;
        private UITemplateTimeDecayProvider provider;

        [SetUp]
        public void Setup()
        {
            this.mockController = new MockSessionController();
            this.provider = new UITemplateTimeDecayProvider(this.mockController, null);
        }

        [Test]
        public void GetTimeSinceLastPlay_ReturnsCorrectTimeSpan()
        {
            // Arrange
            var lastSessionEndTime = DateTime.UtcNow.AddHours(-5);
            this.mockController.TestSessionHistory = new List<SessionData>
            {
                new SessionData { EndTime = lastSessionEndTime }
            };

            // Act
            var result = this.provider.GetTimeSinceLastPlay();

            // Assert
            Assert.GreaterOrEqual(result.TotalHours, 4.9);
            Assert.LessOrEqual(result.TotalHours, 5.1);
        }

        [Test]
        public void GetTimeSinceLastPlay_ReturnsZero_WhenNoHistory()
        {
            // Arrange
            this.mockController.TestSessionHistory = new List<SessionData>();

            // Act
            var result = this.provider.GetTimeSinceLastPlay();

            // Assert
            Assert.AreEqual(TimeSpan.Zero, result);
        }

        [Test]
        public void GetTimeSinceLastPlay_ReturnsZero_WhenControllerIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateTimeDecayProvider(null, null);

            // Act
            var result = nullProvider.GetTimeSinceLastPlay();

            // Assert
            Assert.AreEqual(TimeSpan.Zero, result);
        }

        [Test]
        public void GetTimeSinceLastPlay_ReturnsZero_ForFutureEndTime()
        {
            // Arrange
            var futureEndTime = DateTime.UtcNow.AddHours(1);
            this.mockController.TestSessionHistory = new List<SessionData>
            {
                new SessionData { EndTime = futureEndTime }
            };

            // Act
            var result = this.provider.GetTimeSinceLastPlay();

            // Assert
            Assert.AreEqual(TimeSpan.Zero, result);
        }

        [Test]
        public void GetLastPlayTime_ReturnsCorrectTime()
        {
            // Arrange
            var expectedTime = DateTime.UtcNow.AddDays(-3);
            this.mockController.TestSessionHistory = new List<SessionData>
            {
                new SessionData { EndTime = DateTime.UtcNow.AddDays(-5) },
                new SessionData { EndTime = expectedTime } // Most recent
            };

            // Act
            var result = this.provider.GetLastPlayTime();

            // Assert
            Assert.AreEqual(expectedTime, result);
        }

        [Test]
        public void GetLastPlayTime_ReturnsDefaultTime_WhenNoHistory()
        {
            // Arrange
            this.mockController.TestSessionHistory = new List<SessionData>();

            // Act
            var result = this.provider.GetLastPlayTime();

            // Assert
            var expectedTime = DateTime.UtcNow.AddDays(-UITemplateIntegrationConstants.DEFAULT_DAYS_AGO);
            Assert.GreaterOrEqual(result, expectedTime.AddSeconds(-1));
            Assert.LessOrEqual(result, expectedTime.AddSeconds(1));
        }

        [Test]
        public void GetDaysAwayFromGame_ReturnsCorrectDays()
        {
            // Arrange
            var daysAgo = 7;
            var lastSessionEndTime = DateTime.UtcNow.AddDays(-daysAgo);
            this.mockController.TestSessionHistory = new List<SessionData>
            {
                new SessionData { EndTime = lastSessionEndTime }
            };

            // Act
            var result = this.provider.GetDaysAwayFromGame();

            // Assert
            Assert.AreEqual(daysAgo, result);
        }

        [Test]
        public void GetDaysAwayFromGame_ReturnsZero_ForRecentPlay()
        {
            // Arrange
            var lastSessionEndTime = DateTime.UtcNow.AddHours(-2);
            this.mockController.TestSessionHistory = new List<SessionData>
            {
                new SessionData { EndTime = lastSessionEndTime }
            };

            // Act
            var result = this.provider.GetDaysAwayFromGame();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetDaysAwayFromGame_ReturnsZero_WhenNoHistory()
        {
            // Arrange
            this.mockController.TestSessionHistory = new List<SessionData>();

            // Act
            var result = this.provider.GetDaysAwayFromGame();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetDaysAwayFromGame_HandlesPartialDays()
        {
            // Arrange
            var lastSessionEndTime = DateTime.UtcNow.AddDays(-2.7); // 2.7 days ago
            this.mockController.TestSessionHistory = new List<SessionData>
            {
                new SessionData { EndTime = lastSessionEndTime }
            };

            // Act
            var result = this.provider.GetDaysAwayFromGame();

            // Assert
            Assert.AreEqual(2, result); // Should floor to 2 days
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenControllerIsNull()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                new UITemplateTimeDecayProvider(null, null));
        }

        [Test]
        public void GetTimeSinceLastPlay_HandlesException()
        {
            // Arrange
            this.mockController.ThrowException = true;

            // Act
            var result = this.provider.GetTimeSinceLastPlay();

            // Assert
            Assert.AreEqual(TimeSpan.Zero, result);
        }

        [Test]
        public void GetLastPlayTime_HandlesException()
        {
            // Arrange
            this.mockController.ThrowException = true;

            // Act
            var result = this.provider.GetLastPlayTime();

            // Assert
            var expectedTime = DateTime.UtcNow.AddDays(-UITemplateIntegrationConstants.DEFAULT_DAYS_AGO);
            Assert.GreaterOrEqual(result, expectedTime.AddSeconds(-1));
            Assert.LessOrEqual(result, expectedTime.AddSeconds(1));
        }

        // Mock controller for testing
        private class MockSessionController : UITemplateGameSessionDataController
        {
            public List<SessionData> TestSessionHistory { get; set; }
            public bool ThrowException { get; set; }

            public MockSessionController() : base(null, null, null, null, null, null) { }

            public override List<SessionData> GetDetailedSessionHistory()
            {
                if (this.ThrowException)
                    throw new Exception("Test exception");

                return this.TestSessionHistory;
            }
        }
    }
}