namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI
{
    using GameFoundation.DI;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.DI;
    using TheOneStudio.DynamicUserDifficulty.Providers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData;
    using VContainer;
    using UnityEngine.Scripting;

    /// <summary>
    /// VContainer module for registering Dynamic User Difficulty with UITemplate integration.
    /// This module provides complete integration with UITemplate's data system and signals.
    /// </summary>
    [Preserve]
    public class DynamicDifficultyUITemplateModule : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            // Register the local data storage
            builder.Register<UITemplateDifficultyData>(Lifetime.Singleton);

            // Register the controller (implements all provider interfaces)
            builder.Register<UITemplateDifficultyDataController>(Lifetime.Singleton)
                .As<UITemplateDifficultyDataController>()
                .As<IDifficultyDataProvider>()
                .As<IWinStreakProvider>()
                .As<ITimeDecayProvider>()
                .As<IRageQuitProvider>()
                .As<ILevelProgressProvider>()
                .As<ITickable>();

            // Register the adapter for signal handling
            builder.Register<UITemplateDifficultyAdapter>(Lifetime.Singleton)
                .As<IInitializable>();

            // Install the core Dynamic Difficulty module
            // This registers the service, calculator, and modifiers
            new DynamicDifficultyModule().Install(builder);
        }
    }

    /// <summary>
    /// Extension methods for easy registration in UITemplate's VContainer setup
    /// </summary>
    public static class DynamicDifficultyUITemplateExtensions
    {
        /// <summary>
        /// Register Dynamic User Difficulty with UITemplate integration.
        /// Add this to your UITemplateVContainer or GameLifetimeScope.
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <returns>Container builder for chaining</returns>
        public static IContainerBuilder RegisterDynamicDifficultyUITemplate(this IContainerBuilder builder)
        {
            new DynamicDifficultyUITemplateModule().Install(builder);
            return builder;
        }
    }
}