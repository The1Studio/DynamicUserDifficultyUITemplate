namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI
{
    using TheOneStudio.DynamicUserDifficulty.DI;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers;
    using VContainer;

    /// <summary>
    /// VContainer extension methods for registering Dynamic User Difficulty with UITemplate integration.
    /// This module provides complete integration with UITemplate's data system and signals.
    /// </summary>
    public static class DynamicDifficultyUITemplateModule
    {
        /// <summary>
        /// Register Dynamic User Difficulty with UITemplate integration.
        /// Add this to your UITemplateVContainer or GameLifetimeScope.
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <returns>Container builder for chaining</returns>
        public static void RegisterDynamicDifficultyUITemplate(this IContainerBuilder builder)
        {
            // Register focused providers - Single Responsibility Principle
            // Each provider handles only its specific concern

            // Difficulty data persistence
            builder.Register<UITemplateDifficultyDataProvider>(Lifetime.Singleton)
                .As<IDifficultyDataProvider>();

            // Win/loss streak tracking
            builder.Register<UITemplateWinStreakProvider>(Lifetime.Singleton)
                .As<IWinStreakProvider>();

            // Time-based decay
            builder.Register<UITemplateTimeDecayProvider>(Lifetime.Singleton)
                .As<ITimeDecayProvider>();

            // Rage quit detection
            builder.Register<UITemplateRageQuitProvider>(Lifetime.Singleton)
                .As<IRageQuitProvider>();

            // Level progression tracking
            builder.Register<UITemplateLevelProgressProvider>(Lifetime.Singleton)
                .As<ILevelProgressProvider>();

            // Install the core Dynamic Difficulty module
            // This registers the service, calculator, and modifiers
            builder.RegisterDynamicDifficulty();
        }
    }
}