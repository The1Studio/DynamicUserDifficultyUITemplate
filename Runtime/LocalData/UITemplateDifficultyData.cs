namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Interfaces;
    using Newtonsoft.Json;
    using Sirenix.OdinInspector;
    using TheOneStudio.DynamicUserDifficulty.Models;
    using TheOneStudio.UITemplate.UITemplate.Models.LocalDatas;
    using UnityEngine.Scripting;

    /// <summary>
    /// Local data storage for Dynamic User Difficulty using UITemplate's data system.
    /// Stores current difficulty and session tracking data.
    /// </summary>
    [Preserve]
    public class UITemplateDifficultyData : ILocalData, IUITemplateLocalData
    {
        [JsonProperty] [ShowInInspector] public float CurrentDifficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;

        [JsonProperty] [ShowInInspector] public int WinStreak { get; set; } = 0;
        [JsonProperty] [ShowInInspector] public int LossStreak { get; set; } = 0;
        [JsonProperty] [ShowInInspector] public int TotalWins { get; set; } = 0;
        [JsonProperty] [ShowInInspector] public int TotalLosses { get; set; } = 0;

        [JsonProperty] [ShowInInspector] public DateTime LastPlayTime { get; set; } = DateTime.Now;
        [JsonProperty] [ShowInInspector] public DateTime SessionStartTime { get; set; } = DateTime.Now;

        [JsonProperty] [ShowInInspector] public QuitType LastQuitType { get; set; } = QuitType.Normal;
        [JsonProperty] [ShowInInspector] public float LastSessionDuration { get; set; } = 0f;

        [JsonProperty] [ShowInInspector] public int CurrentLevelAttempts { get; set; } = 1;
        [JsonProperty] [ShowInInspector] public float LastCompletionTime { get; set; } = 60f;

        [JsonProperty] [ShowInInspector] public Queue<SessionInfo> RecentSessions { get; set; } = new Queue<SessionInfo>();

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