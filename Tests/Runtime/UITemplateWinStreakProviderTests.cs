namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Tests
{
    using NUnit.Framework;
    using TheOne.Features.WinStreak.Core.Controller;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers;
    using UnityEngine;

    [TestFixture]
    public class UITemplateWinStreakProviderTests
    {
        private MockWinStreakController mockController;
        private UITemplateWinStreakProvider provider;

        [SetUp]
        public void Setup()
        {
            this.mockController = new MockWinStreakController();
            this.provider = new UITemplateWinStreakProvider(this.mockController, null);
        }

        [Test]
        public void GetWinStreak_ReturnsCorrectValue()
        {
            // Arrange
            this.mockController.Streak = 5;

            // Act
            var result = this.provider.GetWinStreak();

            // Assert
            Assert.AreEqual(5, result);
        }

        [Test]
        public void GetWinStreak_ReturnsZero_WhenControllerIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateWinStreakProvider(null, null);

            // Act
            var result = nullProvider.GetWinStreak();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetLossStreak_ReturnsCorrectValue()
        {
            // Arrange
            this.mockController.LossStreak = 3;

            // Act
            var result = this.provider.GetLossStreak();

            // Assert
            Assert.AreEqual(3, result);
        }

        [Test]
        public void GetLossStreak_ReturnsZero_WhenControllerIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateWinStreakProvider(null, null);

            // Act
            var result = nullProvider.GetLossStreak();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetMaxWinStreak_ReturnsCorrectValue()
        {
            // Arrange
            this.mockController.MaxStreak = 10;

            // Act
            var result = this.provider.GetMaxWinStreak();

            // Assert
            Assert.AreEqual(10, result);
        }

        [Test]
        public void GetMaxWinStreak_ReturnsZero_WhenControllerIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateWinStreakProvider(null, null);

            // Act
            var result = nullProvider.GetMaxWinStreak();

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetTotalWins_ReturnsCorrectValue()
        {
            // Arrange
            this.mockController.TotalWins = 25;

            // Act
            var result = this.provider.GetTotalWins();

            // Assert
            Assert.AreEqual(25, result);
        }

        [Test]
        public void GetTotalLosses_ReturnsCorrectValue()
        {
            // Arrange
            this.mockController.TotalLosses = 15;

            // Act
            var result = this.provider.GetTotalLosses();

            // Assert
            Assert.AreEqual(15, result);
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenControllerIsNull()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                new UITemplateWinStreakProvider(null, null));
        }

        // Mock controller for testing
        private class MockWinStreakController : WinStreakLocalDataController
        {
            public override int Streak { get; set; }
            public override int LossStreak { get; set; }
            public override int MaxStreak { get; set; }
            public override int TotalWins { get; set; }
            public override int TotalLosses { get; set; }

            public MockWinStreakController() : base(null, null) { }
        }
    }
}