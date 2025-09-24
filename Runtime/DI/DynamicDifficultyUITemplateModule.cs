namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI
{
    using TheOneStudio.DynamicUserDifficulty.DI;
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
            // Register the adapter for signal handling
            builder.Register<UITemplateDifficultyAdapter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            // Install the core Dynamic Difficulty module
            // This registers the service, calculator, and modifiers
            builder.RegisterDynamicDifficulty();
        }
    }
}