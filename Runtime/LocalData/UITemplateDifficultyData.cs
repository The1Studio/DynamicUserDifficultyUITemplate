namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData
{
    using System;
    using GameFoundation.Scripts.Interfaces;
    using Newtonsoft.Json;
    using Sirenix.OdinInspector;
    using TheOneStudio.DynamicUserDifficulty.Core;
    using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers;
    using TheOneStudio.UITemplate.UITemplate.Models.LocalDatas;
    using UnityEngine.Scripting;

    /// <summary>
    /// Local data storage for Dynamic User Difficulty using UITemplate's data system.
    /// ONLY stores the current difficulty value and loss streak - all other data comes from existing UITemplate/TheOne controllers.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyData : ILocalData, IUITemplateLocalData
    {
        // Core difficulty value - this is the ONLY value we need to store
        [JsonProperty] [ShowInInspector] public float CurrentDifficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;

        // Loss streak - not tracked by WinStreakController (only tracks wins)
        [JsonProperty] [ShowInInspector] public int LossStreak { get; set; } = 0;

        public void Init()
        {
            // Initialize any runtime data if needed
        }

        public void OnDataLoaded()
        {
            // Called when data is loaded from storage
            // Can be used to migrate old data formats if needed
        }

        public Type ControllerType => typeof(UITemplateDifficultyDataController);
    }
}