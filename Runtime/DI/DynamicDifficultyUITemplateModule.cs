namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI
{
    using TheOneStudio.DynamicUserDifficulty.DI;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Providers;
    using VContainer;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration;
    using UnityEngine;

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
            Debug.Log("DifficultyAdapter");

            // Install the core Dynamic Difficulty module
            // This registers the service, calculator, and modifiers
            builder.RegisterDynamicDifficulty();

            // Difficulty data persistence (delegates to controller)
            builder.Register<DifficultyDataProvider>(Lifetime.Singleton)
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

            // Session pattern analysis (advanced features)
            builder.Register<UITemplateSessionPatternProvider>(Lifetime.Singleton)
                .As<ISessionPatternProvider>();

            // Register the adapter that handles business logic
            builder.Register<DifficultyAdapter>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.AutoResolve<DifficultyAdapter>();
        }
    }
}