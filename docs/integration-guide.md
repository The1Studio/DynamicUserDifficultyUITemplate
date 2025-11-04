# Integration Guide

## Quick Start (5 Minutes)

### Step 1: Add Package Dependency

The package is already available as a Unity Package Manager (UPM) package. Add it to your project:

**Method A: Via Package Manager (Recommended)**
1. Open Unity Package Manager (Window → Package Manager)
2. Click the `+` button
3. Select "Add package from git URL"
4. Enter: `git@github.com:The1Studio/DynamicUserDifficultyUITemplate.git#master`

**Method B: Via manifest.json**
```json
{
  "dependencies": {
    "com.theone.dynamicuserdifficulty.uitemplateintegration": "https://github.com/The1Studio/DynamicUserDifficultyUITemplate.git#master"
  }
}
```

**Method C: As Submodule (Already configured in your project)**
```bash
# Package is already at:
Packages/DynamicUserDifficultyUITemplate/
```

### Step 2: Register with VContainer

In your game's lifetime scope (e.g., `GameLifetimeScope.cs` or `UITemplateVContainer.cs`):

```csharp
using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // One line adds complete 7-modifier difficulty system!
        builder.RegisterDynamicDifficultyUITemplate();

        // Your other registrations...
        builder.Register<GameManager>(Lifetime.Singleton);
        builder.Register<LevelManager>(Lifetime.Singleton);
    }
}
```

### Step 3: Inject and Use

In any class that needs difficulty information:

```csharp
using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration;
using VContainer;

public class LevelManager
{
    [Inject] private DifficultyAdapter difficultyAdapter;

    public void StartLevel()
    {
        // Get current difficulty (1-10 scale)
        float difficulty = difficultyAdapter.CurrentDifficulty;

        // Get adjusted game parameters
        var parameters = difficultyAdapter.GetAdjustedParameters();

        // Apply to your game
        ConfigureLevel(difficulty, parameters);
    }
}
```

**That's it!** The system is now:
- ✅ Tracking player performance automatically
- ✅ Adjusting difficulty based on 7 modifiers
- ✅ Persisting data across sessions
- ✅ Handling all UITemplate signals

---

## Detailed Integration

### Understanding the Architecture

The integration works through three layers:

```
Your Game Code
    ↓ (inject)
DifficultyAdapter
    ↓ (uses)
UITemplateDifficultyDataController
    ↓ (implements 5 provider interfaces)
Dynamic Difficulty Service (Core)
    ↓ (uses 7 modifiers)
Difficulty Calculation
```

### Automatic Signal Handling

The system automatically subscribes to UITemplate signals. No manual event handling required!

**Automatically Tracked Events**:
- `LevelStartedSignal` → Tracks level attempts, resets timer
- `LevelEndedSignal` → Records win/loss, updates all metrics
- `LevelSkippedSignal` → Treated as loss for difficulty
- `ApplicationQuitSignal` → Analyzes quit type, session data
- `ApplicationPauseSignal` → Handles background/foreground

**What This Means**:
You don't need to manually call any tracking methods. Just emit the standard UITemplate signals and the system handles the rest!

---

## Common Integration Scenarios

### Scenario 1: Match-3 Puzzle Game

**Goal**: Adjust move count, time limits, and hint availability based on player performance.

```csharp
public class PuzzleLevelManager
{
    [Inject] private DifficultyAdapter difficultyAdapter;

    public void StartPuzzleLevel(int levelNumber)
    {
        float difficulty = difficultyAdapter.CurrentDifficulty;

        // Difficulty scale: 1.0 (easiest) to 10.0 (hardest)
        int movesAvailable = CalculateMoves(difficulty);
        float timeLimit = CalculateTimeLimit(difficulty);
        int hintsAvailable = CalculateHints(difficulty);

        ConfigurePuzzle(levelNumber, movesAvailable, timeLimit, hintsAvailable);
    }

    private int CalculateMoves(float difficulty)
    {
        // More moves for lower difficulty
        return Mathf.RoundToInt(30 - (difficulty * 2));
        // Difficulty 1.0 → 28 moves
        // Difficulty 5.0 → 20 moves
        // Difficulty 10.0 → 10 moves
    }

    private float CalculateTimeLimit(float difficulty)
    {
        // More time for lower difficulty
        return 180f - (difficulty * 10f);
        // Difficulty 1.0 → 170s
        // Difficulty 5.0 → 130s
        // Difficulty 10.0 → 80s
    }

    private int CalculateHints(float difficulty)
    {
        // More hints for lower difficulty
        if (difficulty <= 3.0f) return 3;
        if (difficulty <= 6.0f) return 2;
        if (difficulty <= 8.0f) return 1;
        return 0;
    }
}
```

### Scenario 2: Action/Platformer Game

**Goal**: Adjust enemy speed, spawn rate, and player health based on difficulty.

```csharp
public class ActionLevelManager
{
    [Inject] private DifficultyAdapter difficultyAdapter;

    public void StartActionLevel(string levelId)
    {
        float difficulty = difficultyAdapter.CurrentDifficulty;

        var config = new LevelConfiguration
        {
            EnemySpeed = 1.0f + (difficulty * 0.15f),
            EnemySpawnRate = 2.0f - (difficulty * 0.1f),
            EnemyDamage = 10f + (difficulty * 2f),
            PlayerHealth = 100f - (difficulty * 5f),
            PlayerSpeed = 5.0f + ((10f - difficulty) * 0.1f)
        };

        ConfigureLevel(levelId, config);
    }
}
```

### Scenario 3: Strategy/Tower Defense

**Goal**: Adjust enemy count, wave frequency, and resource generation.

```csharp
public class TowerDefenseManager
{
    [Inject] private DifficultyAdapter difficultyAdapter;

    public void StartWave(int waveNumber)
    {
        float difficulty = difficultyAdapter.CurrentDifficulty;

        var waveConfig = new WaveConfiguration
        {
            EnemyCount = CalculateEnemyCount(waveNumber, difficulty),
            EnemyHealth = CalculateEnemyHealth(difficulty),
            WaveInterval = CalculateWaveInterval(difficulty),
            ResourceMultiplier = CalculateResourceMultiplier(difficulty)
        };

        SpawnWave(waveConfig);
    }

    private int CalculateEnemyCount(int wave, float difficulty)
    {
        int baseCount = 5 + (wave * 2);
        float difficultyMultiplier = 1.0f + ((difficulty - 5.0f) * 0.2f);
        return Mathf.RoundToInt(baseCount * difficultyMultiplier);
    }

    private float CalculateEnemyHealth(float difficulty)
    {
        return 50f * (1.0f + (difficulty * 0.15f));
    }

    private float CalculateWaveInterval(float difficulty)
    {
        // Faster waves at higher difficulty
        return 30f - (difficulty * 2f);
    }

    private float CalculateResourceMultiplier(float difficulty)
    {
        // More resources at lower difficulty
        return 1.5f - (difficulty * 0.05f);
    }
}
```

### Scenario 4: Endless Runner

**Goal**: Adjust obstacle frequency, speed increase rate, and power-up spawn.

```csharp
public class EndlessRunnerManager
{
    [Inject] private DifficultyAdapter difficultyAdapter;

    private float currentSpeed;
    private float obstacleFrequency;

    public void StartRun()
    {
        float difficulty = difficultyAdapter.CurrentDifficulty;

        currentSpeed = CalculateInitialSpeed(difficulty);
        obstacleFrequency = CalculateObstacleFrequency(difficulty);
        float powerUpChance = CalculatePowerUpChance(difficulty);

        ConfigureRunner(currentSpeed, obstacleFrequency, powerUpChance);
    }

    private float CalculateInitialSpeed(float difficulty)
    {
        // Faster start for higher difficulty
        return 5.0f + (difficulty * 0.5f);
    }

    private float CalculateObstacleFrequency(float difficulty)
    {
        // More frequent obstacles at higher difficulty
        return 2.0f - (difficulty * 0.1f);
    }

    private float CalculatePowerUpChance(float difficulty)
    {
        // More power-ups at lower difficulty
        return 0.3f - (difficulty * 0.02f);
    }
}
```

---

## Advanced Usage

### Monitoring Difficulty Changes

Subscribe to `DifficultyChangedSignal` to react to difficulty updates:

```csharp
public class DifficultyMonitor : IInitializable
{
    [Inject] private ISignalBus signalBus;

    public void Initialize()
    {
        signalBus.Subscribe<DifficultyChangedSignal>(OnDifficultyChanged);
    }

    private void OnDifficultyChanged(DifficultyChangedSignal signal)
    {
        Debug.Log($"Difficulty: {signal.OldDifficulty} → {signal.NewDifficulty}");
        Debug.Log($"Active modifiers: {signal.AppliedModifiers.Count}/7");

        // Update UI
        UpdateDifficultyIndicator(signal.NewDifficulty);

        // Show notification to player
        if (Mathf.Abs(signal.DifficultyDelta) > 1.0f)
        {
            ShowDifficultyChangeNotification(signal);
        }

        // Track analytics
        TrackDifficultyChange(signal);
    }
}
```

### Accessing Detailed Provider Data

For analytics or debugging, access detailed player metrics:

```csharp
public class PlayerAnalytics
{
    [Inject] private UITemplateDifficultyDataController controller;

    public void LogPlayerMetrics()
    {
        var metrics = new Dictionary<string, object>
        {
            ["difficulty"] = controller.GetCurrentDifficulty(),
            ["win_streak"] = controller.GetWinStreak(),
            ["loss_streak"] = controller.GetLossStreak(),
            ["total_wins"] = controller.GetTotalWins(),
            ["total_losses"] = controller.GetTotalLosses(),
            ["completion_rate"] = controller.GetCompletionRate(),
            ["days_away"] = controller.GetDaysAway(),
            ["session_duration"] = controller.GetSessionDuration().TotalMinutes,
            ["rage_quit_count"] = controller.GetRecentRageQuitCount()
        };

        AnalyticsService.Track("player_session_summary", metrics);
    }
}
```

### Custom Difficulty Adjustment

Override difficulty for special events or testing:

```csharp
public class SpecialEventManager
{
    [Inject] private UITemplateDifficultyDataController controller;

    public void StartEasyMode()
    {
        // Temporarily reduce difficulty for special event
        float currentDifficulty = controller.GetCurrentDifficulty();
        controller.SetCurrentDifficulty(currentDifficulty * 0.7f);

        Debug.Log("Easy mode activated for special event!");
    }

    public void StartChallengeMode()
    {
        // Increase difficulty for challenge mode
        controller.SetCurrentDifficulty(9.0f);

        Debug.Log("Challenge mode activated!");
    }

    public void ResetToAutomatic()
    {
        // Let the system calculate difficulty automatically
        // (it will recalculate on next level event)
        Debug.Log("Automatic difficulty restored");
    }
}
```

### Debug Tools

Create debug tools for testing:

```csharp
#if UNITY_EDITOR
public class DifficultyDebugger : MonoBehaviour
{
    [Inject] private UITemplateDifficultyDataController controller;
    [Inject] private DifficultyAdapter adapter;

    [ContextMenu("Log Current State")]
    private void LogCurrentState()
    {
        Debug.Log("=== Difficulty System State ===");
        Debug.Log($"Current Difficulty: {controller.GetCurrentDifficulty()}");
        Debug.Log($"Win Streak: {controller.GetWinStreak()}");
        Debug.Log($"Loss Streak: {controller.GetLossStreak()}");
        Debug.Log($"Total Wins: {controller.GetTotalWins()}");
        Debug.Log($"Total Losses: {controller.GetTotalLosses()}");
        Debug.Log($"Completion Rate: {controller.GetCompletionRate():P1}");
        Debug.Log($"Days Away: {controller.GetDaysAway()}");
        Debug.Log($"Session Duration: {controller.GetSessionDuration()}");
    }

    [ContextMenu("Reset All Data")]
    private void ResetAllData()
    {
        controller.SetCurrentDifficulty(3.0f);
        // Note: Full reset requires clearing UITemplate local data
        Debug.Log("Difficulty reset to default (3.0)");
    }

    [ContextMenu("Simulate Win Streak")]
    private void SimulateWinStreak()
    {
        for (int i = 0; i < 5; i++)
        {
            adapter.RecordLevelCompletion($"Level_{i}", 30f, won: true);
        }
        Debug.Log("Simulated 5 consecutive wins");
    }

    [ContextMenu("Simulate Loss Streak")]
    private void SimulateLossStreak()
    {
        for (int i = 0; i < 3; i++)
        {
            adapter.RecordLevelCompletion($"Level_{i}", 60f, won: false);
        }
        Debug.Log("Simulated 3 consecutive losses");
    }

    [ContextMenu("Simulate Rage Quit")]
    private void SimulateRageQuit()
    {
        adapter.RecordSessionEnd(QuitType.RageQuit);
        Debug.Log("Simulated rage quit");
    }
}
#endif
```

---

## UITemplate Signal Integration

### Required Signals

Ensure your UITemplate project emits these signals:

#### LevelStartedSignal
```csharp
public class LevelStartedSignal
{
    public string LevelId { get; }
    public float LevelDifficulty { get; }
    public DateTime Timestamp { get; }
}
```

#### LevelEndedSignal
```csharp
public class LevelEndedSignal
{
    public string LevelId { get; }
    public bool Won { get; }
    public float CompletionTime { get; }
    public int Score { get; }
    public DateTime Timestamp { get; }
}
```

#### ApplicationQuitSignal
```csharp
public class ApplicationQuitSignal
{
    public string QuitReason { get; }
    public DateTime Timestamp { get; }
}
```

### Emitting Signals

Example of emitting signals from your game:

```csharp
public class LevelController
{
    [Inject] private ISignalBus signalBus;

    public void StartLevel(string levelId)
    {
        signalBus.Fire(new LevelStartedSignal
        {
            LevelId = levelId,
            LevelDifficulty = currentDifficulty,
            Timestamp = DateTime.UtcNow
        });
    }

    public void EndLevel(bool won, float completionTime)
    {
        signalBus.Fire(new LevelEndedSignal
        {
            LevelId = currentLevelId,
            Won = won,
            CompletionTime = completionTime,
            Score = currentScore,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

---

## Testing Integration

### Unit Testing Provider Data

```csharp
[Test]
public void TestDifficultyCalculation()
{
    // Arrange
    var controller = CreateController();
    controller.SetCurrentDifficulty(5.0f);

    // Simulate wins
    for (int i = 0; i < 3; i++)
    {
        adapter.RecordLevelCompletion($"Level_{i}", 30f, won: true);
    }

    // Assert
    Assert.AreEqual(3, controller.GetWinStreak());
    Assert.Greater(controller.GetCurrentDifficulty(), 5.0f);
}
```

### Integration Testing

```csharp
[UnityTest]
public IEnumerator TestDifficultyAdjustment()
{
    // Start level
    signalBus.Fire(new LevelStartedSignal { LevelId = "Test_01" });
    yield return new WaitForSeconds(0.1f);

    // Complete level
    signalBus.Fire(new LevelEndedSignal
    {
        LevelId = "Test_01",
        Won = true,
        CompletionTime = 30f
    });
    yield return new WaitForSeconds(0.1f);

    // Verify difficulty increased
    Assert.Greater(difficultyAdapter.CurrentDifficulty, initialDifficulty);
}
```

---

## Performance Optimization

### Best Practices

1. **Cache Difficulty Values**: Don't query `CurrentDifficulty` every frame
```csharp
private float cachedDifficulty;
private float nextDifficultyCheck;

void Update()
{
    if (Time.time > nextDifficultyCheck)
    {
        cachedDifficulty = difficultyAdapter.CurrentDifficulty;
        nextDifficultyCheck = Time.time + 5f; // Check every 5 seconds
    }
}
```

2. **Subscribe to Changes**: React to difficulty changes instead of polling
```csharp
signalBus.Subscribe<DifficultyChangedSignal>(signal =>
{
    cachedDifficulty = signal.NewDifficulty;
    UpdateGameParameters();
});
```

3. **Batch Updates**: Update multiple parameters together
```csharp
public void ApplyDifficultySettings(float difficulty)
{
    var parameters = CalculateAllParameters(difficulty);
    ApplyParametersBatch(parameters);
}
```

---

## Troubleshooting

### Difficulty Not Changing

**Problem**: Difficulty stays at default value.

**Solutions**:
1. Verify VContainer registration:
```csharp
builder.RegisterDynamicDifficultyUITemplate();
```

2. Check signal emissions:
```csharp
Debug.Log("Emitting LevelEndedSignal");
signalBus.Fire(new LevelEndedSignal { /* data */ });
```

3. Enable debug logs in DifficultyAdapter

### Provider Data Not Persisting

**Problem**: Data resets every session.

**Solutions**:
1. Verify UITemplate's local data system is properly configured
2. Check UITemplateDifficultyData is included in save data
3. Ensure data controller Save() method is called

### Modifiers Not Applied

**Problem**: Only some modifiers are affecting difficulty.

**Solutions**:
1. Check provider data availability:
```csharp
Debug.Log($"Win Streak: {controller.GetWinStreak()}");
Debug.Log($"Days Away: {controller.GetDaysAway()}");
```

2. Verify all 7 modifiers are registered in DI
3. Check modifier thresholds are met (e.g., 3+ wins for WinStreakModifier)

---

## Migration from v1.0.0

If upgrading from v1.0.0, no breaking changes exist:

### What's New in v2.0.0
- 3 new modifiers: CompletionRate, LevelProgress, SessionPattern
- Separated provider implementations for better maintainability
- Enhanced session tracking
- Performance optimizations

### Migration Steps
1. Update package reference to v2.0.0
2. No code changes required - fully backward compatible
3. New modifiers activate automatically
4. Existing data structures preserved

---

## Next Steps

1. **Read Architecture Docs**: [architecture.md](architecture.md)
2. **Explore API Reference**: [api-reference.md](api-reference.md)
3. **Review Examples**: See usage examples in README.md
4. **Join Community**: Contact support@theonestudio.com

---

**Last Updated**: November 4, 2025
**Version**: 2.0.0
**Status**: Production Ready ✅
