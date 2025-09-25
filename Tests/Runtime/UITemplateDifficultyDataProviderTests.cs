namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Tests
{
    using NUnit.Framework;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers;
    using UnityEngine;

    [TestFixture]
    public class UITemplateDifficultyDataProviderTests
    {
        private UITemplateDifficultyData difficultyData;
        private UITemplateDifficultyDataProvider provider;

        [SetUp]
        public void Setup()
        {
            // Create ScriptableObject instance for testing
            this.difficultyData = ScriptableObject.CreateInstance<UITemplateDifficultyData>();
            this.provider = new UITemplateDifficultyDataProvider(this.difficultyData, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (this.difficultyData != null)
            {
                Object.DestroyImmediate(this.difficultyData);
            }
        }

        [Test]
        public void GetCurrentDifficulty_ReturnsDefaultValue_WhenNotSet()
        {
            // Arrange
            this.difficultyData.CurrentDifficulty = 0f;

            // Act
            var difficulty = this.provider.GetCurrentDifficulty();

            // Assert
            Assert.AreEqual(0f, difficulty);
        }

        [Test]
        public void GetCurrentDifficulty_ReturnsStoredValue()
        {
            // Arrange
            this.difficultyData.CurrentDifficulty = 5.5f;

            // Act
            var difficulty = this.provider.GetCurrentDifficulty();

            // Assert
            Assert.AreEqual(5.5f, difficulty);
        }

        [Test]
        public void GetCurrentDifficulty_ReturnsDefaultConstant_WhenDataIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateDifficultyDataProvider(null, null);

            // Act
            var difficulty = nullProvider.GetCurrentDifficulty();

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.DEFAULT_DIFFICULTY, difficulty);
        }

        [Test]
        public void SetCurrentDifficulty_StoresValue()
        {
            // Arrange
            const float newDifficulty = 7.2f;

            // Act
            this.provider.SetCurrentDifficulty(newDifficulty);

            // Assert
            Assert.AreEqual(newDifficulty, this.difficultyData.CurrentDifficulty);
        }

        [Test]
        public void SetCurrentDifficulty_ClampsToMinimum()
        {
            // Arrange
            const float belowMin = 0.5f;

            // Act
            this.provider.SetCurrentDifficulty(belowMin);

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.MIN_DIFFICULTY, this.difficultyData.CurrentDifficulty);
        }

        [Test]
        public void SetCurrentDifficulty_ClampsToMaximum()
        {
            // Arrange
            const float aboveMax = 15f;

            // Act
            this.provider.SetCurrentDifficulty(aboveMax);

            // Assert
            Assert.AreEqual(UITemplateIntegrationConstants.MAX_DIFFICULTY, this.difficultyData.CurrentDifficulty);
        }

        [Test]
        public void SetCurrentDifficulty_DoesNotThrow_WhenDataIsNull()
        {
            // Arrange
            var nullProvider = new UITemplateDifficultyDataProvider(null, null);

            // Act & Assert
            Assert.DoesNotThrow(() => nullProvider.SetCurrentDifficulty(5f));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenDifficultyDataIsNull()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                new UITemplateDifficultyDataProvider(null, null));
        }

        [Test]
        public void SetCurrentDifficulty_AcceptsValidRange()
        {
            // Arrange
            float[] validValues = { 1f, 2.5f, 5f, 7.8f, 10f };

            foreach (var value in validValues)
            {
                // Act
                this.provider.SetCurrentDifficulty(value);

                // Assert
                Assert.AreEqual(value, this.difficultyData.CurrentDifficulty,
                    $"Failed for value: {value}");
            }
        }
    }
}