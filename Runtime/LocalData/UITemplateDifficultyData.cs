#nullable enable

namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData
{
    using Newtonsoft.Json;
    using Sirenix.OdinInspector;
    using TheOne.Data;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using UnityEngine.Scripting;

    /// <summary>
    /// Local data storage for Dynamic User Difficulty using UITemplate's data system.
    /// ONLY stores the current difficulty value - all other data comes from existing UITemplate/TheOne controllers.
    /// </summary>
    [Preserve]
    public sealed class UITemplateDifficultyData : IWritableData
    {
        // Core difficulty value - this is the ONLY value we need to store
        [JsonProperty] [ShowInInspector] public float CurrentDifficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;

        // Track previous difficulty for session pattern analysis
        [JsonProperty] [ShowInInspector] public float PreviousDifficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;
    }
}