namespace TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Signals
{
    using UnityEngine.Scripting;

    /// <summary>
    /// Signal fired when difficulty changes.
    /// UI components can subscribe to this signal to react to difficulty changes.
    /// </summary>
    [Preserve]
    public sealed class DifficultyChangedSignal
    {
        public float OldDifficulty { get; }
        public float NewDifficulty { get; }

        public DifficultyChangedSignal(float oldDifficulty, float newDifficulty)
        {
            this.OldDifficulty = oldDifficulty;
            this.NewDifficulty = newDifficulty;
        }
    }
}