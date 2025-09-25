namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers;
    using TheOneStudio.UITemplate.UITemplate.Models;
    using TheOneStudio.UITemplate.UITemplate.Models.Controllers;

    [TestFixture]
    public class UITemplateRageQuitProviderTests
    {
        private MockSessionController mockSessionController;
        private MockLevelController mockLevelController;
        private UITemplateRageQuitProvider provider;

        [SetUp]
        public void Setup()
        {
            this.mockSessionController = new MockSessionController();
            this.mockLevelController = new MockLevelController();
            this.provider = new UITemplateRageQuitProvider(this.mockSessionController, this.mockLevelController);
        }

        [Test]
        public void GetLastQuitType_ReturnsRageQuit_ForShortSessionWithLosses()
        {
            // Arrange
            this.mockSessionController.TestSessionHistory = new List<SessionData>
            {
                new SessionData
                {
                    Duration = DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD - 10, // Below threshold
                    LevelsFailed = 3,
                    LevelsCompleted = 0,
                    LastLevelWon = false,
                    TimeSinceLastLevel = 5
                }
            };

            // Act
            var result = this.provider.GetLastQuitType();

            // Assert
            Assert.AreEqual(QuitType.RageQuit, result);
        }

        [Test]
        public void GetLastQuitType_ReturnsNormal_ForLongSession()
        {
            // Arrange
            this.mockSessionController.TestSessionHistory = new List<SessionData>
            {
                new SessionData
                {
                    Duration = UITemplateIntegrationConstants.NORMAL_SESSION_THRESHOLD_SECONDS + 100,
                    LevelsFailed = 2,
                    LevelsCompleted = 5
                }
            };

            // Act
            var result = this.provider.GetLastQuitType();

            // Assert
            Assert.AreEqual(QuitType.Normal, result);
        }

        [Test]
        public void GetLastQuitType_ReturnsMidPlay_ForMediumSession()
        {
            // Arrange
            this.mockSessionController.TestSessionHistory = new List<SessionData>
            {
                new SessionData
                {
                    Duration = 900, // 15 minutes - between rage quit and normal
                    LevelsFailed = 1,
                    LevelsCompleted = 2
                }
            };

            // Act
            var result = this.provider.GetLastQuitType();

            // Assert
            Assert.AreEqual(QuitType.MidPlay, result);
        }

        [Test]
        public void GetLastQuitType_ReturnsNormal_WhenNoHistory()
        {
            // Arrange
            this.mockSessionController.TestSessionHistory = new List<SessionData>();

            // Act
            var result = this.provider.GetLastQuitType();

            // Assert
            Assert.AreEqual(QuitType.Normal, result);
        }

        [Test]
        public void GetLastQuitType_ReturnsNormal_WhenControllerIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateRageQuitProvider(null, this.mockLevelController);

            // Act
            var result = nullProvider.GetLastQuitType();

            // Assert
            Assert.AreEqual(QuitType.Normal, result);
        }

        [Test]
        public void GetAverageSessionDuration_ReturnsCorrectValue()
        {
            // Arrange
            this.mockSessionController.TestAverageSessionDuration = 120f;

            // Act
            var result = this.provider.GetAverageSessionDuration();

            // Assert
            Assert.AreEqual(120f, result);
        }

        [Test]
        public void GetAverageSessionDuration_ReturnsDefault_WhenControllerIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateRageQuitProvider(null, this.mockLevelController);

            // Act
            var result = nullProvider.GetAverageSessionDuration();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.DEFAULT_SESSION_DURATION_SECONDS, result);
        }

        [Test]
        public void GetCurrentSessionDuration_CalculatesCorrectly()
        {
            // Arrange
            var sessionStartTime = DateTime.UtcNow.AddSeconds(-45);
            this.mockSessionController.SessionStartTime = sessionStartTime;

            // Act
            var result = this.provider.GetCurrentSessionDuration();

            // Assert
            Assert.GreaterOrEqual(result, 44f);
            Assert.LessOrEqual(result, 46f);
        }

        [Test]
        public void GetCurrentSessionDuration_ReturnsZero_WhenNoSessionStarted()
        {
            // Arrange
            this.mockSessionController.SessionStartTime = DateTime.UtcNow;

            // Act
            var result = this.provider.GetCurrentSessionDuration();

            // Assert
            Assert.GreaterOrEqual(result, 0f);
            Assert.LessOrEqual(result, 1f);
        }

        [Test]
        public void GetRecentRageQuitCount_CountsCorrectly()
        {
            // Arrange
            var sessions = new List<SessionData>();

            // Add rage quit sessions
            for (int i = 0; i < 3; i++)
            {
                sessions.Add(new SessionData
                {
                    Duration = DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD - 10,
                    LevelsFailed = 2,
                    LevelsCompleted = 0
                });
            }

            // Add normal sessions
            for (int i = 0; i < 2; i++)
            {
                sessions.Add(new SessionData
                {
                    Duration = 3600,
                    LevelsFailed = 1,
                    LevelsCompleted = 5
                });
            }

            this.mockSessionController.TestSessionHistory = sessions;

            // Act
            var result = this.provider.GetRecentRageQuitCount();

            // Assert
            Assert.AreEqual(3, result);
        }

        [Test]
        public void GetRecentRageQuitCount_LimitsToMaxSessions()
        {
            // Arrange
            var sessions = new List<SessionData>();

            // Add more than MAX_SESSIONS_TO_CHECK rage quit sessions
            for (int i = 0; i < 15; i++)
            {
                sessions.Add(new SessionData
                {
                    Duration = DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD - 10,
                    LevelsFailed = 1,
                    LevelsCompleted = 0
                });
            }

            this.mockSessionController.TestSessionHistory = sessions;

            // Act
            var result = this.provider.GetRecentRageQuitCount();

            // Assert - Should only check last MAX_SESSIONS_TO_CHECK sessions
            Assert.AreEqual(UITemplateIntegrationConstants.MAX_SESSIONS_TO_CHECK, result);
        }

        [Test]
        public void GetRecentRageQuitCount_ReturnsZero_WhenNoHistory()
        {
            // Arrange
            this.mockSessionController.TestSessionHistory = new List<SessionData>();

            // Act
            var result = this.provider.GetRecentRageQuitCount();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenSessionControllerIsNull()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                new UITemplateRageQuitProvider(null, this.mockLevelController));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenLevelControllerIsNull()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                new UITemplateRageQuitProvider(this.mockSessionController, null));
        }

        [Test]
        public void GetLastQuitType_HandlesException()
        {
            // Arrange
            this.mockSessionController.ThrowException = true;

            // Act
            var result = this.provider.GetLastQuitType();

            // Assert
            Assert.AreEqual(QuitType.Normal, result);
        }

        // Mock controllers for testing
        private class MockSessionController : UITemplateGameSessionDataController
        {
            public List<SessionData> TestSessionHistory { get; set; }
            public float TestAverageSessionDuration { get; set; }
            public DateTime SessionStartTime { get; set; }
            public bool ThrowException { get; set; }

            public MockSessionController() : base(null, null, null, null, null, null)
            {
                this.SessionStartTime = DateTime.UtcNow;
            }

            public override List<SessionData> GetDetailedSessionHistory()
            {
                if (this.ThrowException)
                    throw new Exception("Test exception");

                return this.TestSessionHistory;
            }

            public override float GetAverageSessionDuration()
            {
                return this.TestAverageSessionDuration;
            }
        }

        private class MockLevelController : UITemplateLevelDataController
        {
            public MockLevelController() : base(null, null, null, null) { }
        }
    }
}