# API Reference

## Core Classes

### DifficultyAdapter

Main adapter class that integrates Dynamic Difficulty system with UITemplate framework.

#### Namespace
```csharp
TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
```

#### Inheritance
```csharp
public class DifficultyAdapter : IInitializable
```

#### Properties

##### CurrentDifficulty
```csharp
public float CurrentDifficulty { get; }
```
Returns the current difficulty value (1.0 to 10.0) calculated from all 7 modifiers.

**Returns**: `float` - Current difficulty level

**Example**:
```csharp
[Inject] private DifficultyAdapter difficultyAdapter;

void ConfigureLevel()
{
    float difficulty = difficultyAdapter.CurrentDifficulty;
    if (difficulty > 7.0f)
    {
        // High difficulty settings
    }
}
```

#### Methods

##### GetAdjustedParameters()
```csharp
public GameParameters GetAdjustedParameters()
```
Returns game parameters automatically adjusted based on current difficulty.

**Returns**: `GameParameters` - Adjusted parameters for gameplay

**Example**:
```csharp
var parameters = difficultyAdapter.GetAdjustedParameters();
enemySpeed = parameters.EnemySpeed;
timeLimit = parameters.TimeLimit;
```

##### RecordLevelStart(string levelId)
```csharp
public void RecordLevelStart(string levelId)
```
Manually record level start event (usually automatic via signals).

**Parameters**:
- `levelId` (string): Unique identifier for the level

**Example**:
```csharp
difficultyAdapter.RecordLevelStart("Level_01");
```

##### RecordLevelCompletion(string levelId, float completionTime, bool won)
```csharp
public void RecordLevelCompletion(string levelId, float completionTime, bool won)
```
Manually record level completion (usually automatic via signals).

**Parameters**:
- `levelId` (string): Unique identifier for the level
- `completionTime` (float): Time taken to complete in seconds
- `won` (bool): Whether the player won or lost

**Example**:
```csharp
difficultyAdapter.RecordLevelCompletion("Level_01", 45.2f, won: true);
```

##### RecordSessionEnd(QuitType quitType)
```csharp
public void RecordSessionEnd(QuitType quitType)
```
Manually record session end event (usually automatic via signals).

**Parameters**:
- `quitType` (QuitType): Type of quit (Normal, RageQuit, Error)

**Example**:
```csharp
difficultyAdapter.RecordSessionEnd(QuitType.Normal);
```

---

### UITemplateDifficultyDataController

Central data controller implementing all 5 provider interfaces.

#### Namespace
```csharp
TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Controllers
```

#### Inheritance
```csharp
public class UITemplateDifficultyDataController :
    IDifficultyDataProvider,
    IWinStreakProvider,
    ITimeDecayProvider,
    IRageQuitProvider,
    ILevelProgressProvider
```

#### IDifficultyDataProvider Methods

##### GetCurrentDifficulty()
```csharp
float GetCurrentDifficulty()
```
Returns current difficulty value.

**Returns**: `float` - Current difficulty (1.0 to 10.0)

##### SetCurrentDifficulty(float difficulty)
```csharp
void SetCurrentDifficulty(float difficulty)
```
Updates current difficulty value.

**Parameters**:
- `difficulty` (float): New difficulty value (clamped to 1.0-10.0)

#### IWinStreakProvider Methods

##### GetWinStreak()
```csharp
int GetWinStreak()
```
Returns current consecutive win count.

**Returns**: `int` - Number of consecutive wins

##### GetLossStreak()
```csharp
int GetLossStreak()
```
Returns current consecutive loss count.

**Returns**: `int` - Number of consecutive losses

##### GetTotalWins()
```csharp
int GetTotalWins()
```
Returns total wins across all sessions.

**Returns**: `int` - Total win count

##### GetTotalLosses()
```csharp
int GetTotalLosses()
```
Returns total losses across all sessions.

**Returns**: `int` - Total loss count

#### ITimeDecayProvider Methods

##### GetLastPlayTime()
```csharp
DateTime GetLastPlayTime()
```
Returns timestamp of last play session.

**Returns**: `DateTime` - Last play time

##### GetTimeSinceLastPlay()
```csharp
TimeSpan GetTimeSinceLastPlay()
```
Returns time elapsed since last session.

**Returns**: `TimeSpan` - Time since last play

##### GetDaysAway()
```csharp
int GetDaysAway()
```
Returns number of days player was away.

**Returns**: `int` - Days away from game

#### IRageQuitProvider Methods

##### GetLastQuitType()
```csharp
QuitType GetLastQuitType()
```
Returns type of last session end.

**Returns**: `QuitType` - Last quit type (Normal/RageQuit/Error)

##### GetRecentRageQuitCount()
```csharp
int GetRecentRageQuitCount()
```
Returns recent rage quit count.

**Returns**: `int` - Number of recent rage quits

##### GetSessionStartTime()
```csharp
DateTime GetSessionStartTime()
```
Returns current session start timestamp.

**Returns**: `DateTime` - Session start time

##### GetLastSessionDuration()
```csharp
TimeSpan GetLastSessionDuration()
```
Returns duration of last session.

**Returns**: `TimeSpan` - Last session duration

#### ILevelProgressProvider Methods

##### GetCurrentLevelAttempts()
```csharp
int GetCurrentLevelAttempts()
```
Returns attempts on current level.

**Returns**: `int` - Number of attempts

##### GetAverageCompletionTime()
```csharp
float GetAverageCompletionTime()
```
Returns average level completion time.

**Returns**: `float` - Average time in seconds

##### GetLastCompletionTime()
```csharp
float GetLastCompletionTime()
```
Returns last level completion time.

**Returns**: `float` - Time in seconds

##### GetCurrentLevelDifficulty()
```csharp
float GetCurrentLevelDifficulty()
```
Returns current level difficulty rating.

**Returns**: `float` - Level difficulty value

##### GetCompletionRate()
```csharp
float GetCompletionRate()
```
Returns overall completion success rate.

**Returns**: `float` - Completion rate (0.0 to 1.0)

#### Custom Extension Methods

##### GetSessionDuration()
```csharp
TimeSpan GetSessionDuration()
```
Returns current session duration.

**Returns**: `TimeSpan` - Current session duration

##### GetAverageSessionDuration()
```csharp
TimeSpan GetAverageSessionDuration()
```
Returns average session duration across all sessions.

**Returns**: `TimeSpan` - Average session duration

##### GetTotalPlayTime()
```csharp
TimeSpan GetTotalPlayTime()
```
Returns cumulative play time across all sessions.

**Returns**: `TimeSpan` - Total play time

---

### UITemplateDifficultyData

Serializable data model for all difficulty metrics.

#### Namespace
```csharp
TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.LocalData
```

#### Properties

##### Core Difficulty
```csharp
public float CurrentDifficulty { get; set; }
```
Current difficulty value (1.0 to 10.0).

##### Win/Loss Tracking
```csharp
public int WinStreak { get; set; }
public int LossStreak { get; set; }
public int TotalWins { get; set; }
public int TotalLosses { get; set; }
```

##### Session Tracking
```csharp
public DateTime LastPlayTime { get; set; }
public DateTime SessionStartTime { get; set; }
public float SessionDuration { get; set; }
public float LastSessionDuration { get; set; }
public float AverageSessionDuration { get; set; }
public float TotalPlayTime { get; set; }
```

##### Behavioral Analysis
```csharp
public QuitType LastQuitType { get; set; }
public int RecentRageQuitCount { get; set; }
```

##### Level Progress
```csharp
public int CurrentLevelAttempts { get; set; }
public float AverageCompletionTime { get; set; }
public float LastCompletionTime { get; set; }
public float CurrentLevelDifficulty { get; set; }
public float CompletionRate { get; set; }
```

---

## Signals

### DifficultyChangedSignal

Emitted when difficulty value changes.

#### Namespace
```csharp
TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.Signals
```

#### Properties

```csharp
public class DifficultyChangedSignal
{
    public float OldDifficulty { get; }
    public float NewDifficulty { get; }
    public float DifficultyDelta { get; }
    public List<ModifierResult> AppliedModifiers { get; }
    public DateTime Timestamp { get; }
}
```

#### Usage Example
```csharp
[Inject] private ISignalBus signalBus;

void Initialize()
{
    signalBus.Subscribe<DifficultyChangedSignal>(OnDifficultyChanged);
}

void OnDifficultyChanged(DifficultyChangedSignal signal)
{
    Debug.Log($"Difficulty changed: {signal.OldDifficulty} → {signal.NewDifficulty}");
    Debug.Log($"Delta: {signal.DifficultyDelta}");
    Debug.Log($"Active modifiers: {signal.AppliedModifiers.Count}");
}
```

---

## Enums

### QuitType

Represents type of session end.

#### Values
```csharp
public enum QuitType
{
    Normal,      // Normal level completion or menu quit
    RageQuit,    // Quit during level (frustration)
    Error        // Quit due to error/crash
}
```

---

## Extension Methods

### DynamicDifficultyUITemplateModule

VContainer registration extension.

#### Namespace
```csharp
TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI
```

#### Methods

##### RegisterDynamicDifficultyUITemplate()
```csharp
public static void RegisterDynamicDifficultyUITemplate(
    this IContainerBuilder builder)
```
Registers all components for Dynamic Difficulty system.

**Usage**:
```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterDynamicDifficultyUITemplate();
    }
}
```

**Registers**:
- UITemplateDifficultyDataController (as all 5 provider interfaces)
- All specialized providers
- DifficultyAdapter
- IDynamicDifficultyService
- All 7 difficulty modifiers

---

## Constants

### UITemplateIntegrationConstants

Configuration constants for difficulty system.

#### Namespace
```csharp
TheOneStudio.DynamicUserDifficulty.UITemplateIntegration
```

#### Properties

```csharp
public static class UITemplateIntegrationConstants
{
    // Difficulty Range
    public const float MinDifficulty = 1.0f;
    public const float MaxDifficulty = 10.0f;
    public const float DefaultDifficulty = 3.0f;

    // Difficulty Change Limits
    public const float MaxDifficultyChangePerLevel = 2.0f;
    public const float MinDifficultyChangePerLevel = 0.1f;

    // Streak Thresholds
    public const int WinStreakThreshold = 3;
    public const int LossStreakThreshold = 2;

    // Time Decay
    public const int TimeDecayDaysThreshold = 7;
    public const float TimeDecayMaxBonus = -2.0f;

    // Rage Quit Detection
    public const int RageQuitThreshold = 2;
    public const float RageQuitPenaltyReduction = -1.0f;

    // Session Analysis
    public const float MinSessionDuration = 60.0f; // seconds
    public const float ShortSessionThreshold = 300.0f; // 5 minutes

    // Completion Rate
    public const float LowCompletionRateThreshold = 0.3f;
    public const float HighCompletionRateThreshold = 0.7f;
}
```

---

## Usage Examples

### Complete Integration Example

```csharp
using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration;
using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register complete difficulty system
        builder.RegisterDynamicDifficultyUITemplate();

        // Register game services
        builder.Register<GameManager>(Lifetime.Singleton);
        builder.Register<LevelManager>(Lifetime.Singleton);
    }
}

public class LevelManager
{
    [Inject] private DifficultyAdapter difficultyAdapter;
    [Inject] private ISignalBus signalBus;

    public void StartLevel(string levelId)
    {
        // Get current difficulty
        float difficulty = difficultyAdapter.CurrentDifficulty;

        // Get adjusted parameters
        var parameters = difficultyAdapter.GetAdjustedParameters();

        // Configure level
        ConfigureLevel(levelId, difficulty, parameters);
    }

    private void ConfigureLevel(string levelId, float difficulty,
                               GameParameters parameters)
    {
        // Apply difficulty settings
        enemySpeed = parameters.EnemySpeed;
        timeLimit = parameters.TimeLimit;
        hintsAvailable = parameters.HintCount;

        Debug.Log($"Level {levelId} configured for difficulty {difficulty}");
    }
}

public class GameManager : IInitializable
{
    [Inject] private ISignalBus signalBus;

    public void Initialize()
    {
        // Subscribe to difficulty changes
        signalBus.Subscribe<DifficultyChangedSignal>(OnDifficultyChanged);
    }

    private void OnDifficultyChanged(DifficultyChangedSignal signal)
    {
        Debug.Log($"Difficulty updated: {signal.NewDifficulty}");

        // Update UI
        UpdateDifficultyDisplay(signal.NewDifficulty);

        // Log analytics
        AnalyticsService.TrackDifficultyChange(
            signal.OldDifficulty,
            signal.NewDifficulty,
            signal.AppliedModifiers
        );
    }
}
```

### Accessing Provider Data

```csharp
public class AnalyticsManager
{
    [Inject] private UITemplateDifficultyDataController controller;

    public void TrackPlayerProgress()
    {
        // Get comprehensive player data
        var data = new PlayerProgressData
        {
            CurrentDifficulty = controller.GetCurrentDifficulty(),
            WinStreak = controller.GetWinStreak(),
            TotalWins = controller.GetTotalWins(),
            CompletionRate = controller.GetCompletionRate(),
            SessionDuration = controller.GetSessionDuration(),
            DaysAway = controller.GetDaysAway(),
            RageQuitCount = controller.GetRecentRageQuitCount()
        };

        // Send to analytics
        AnalyticsService.Track("player_progress", data);
    }
}
```

### Custom Difficulty Adjustments

```csharp
public class DifficultyDebugger
{
    [Inject] private UITemplateDifficultyDataController controller;
    [Inject] private DifficultyAdapter adapter;

    public void SetEasyMode()
    {
        controller.SetCurrentDifficulty(1.0f);
        Debug.Log("Set to easy mode");
    }

    public void SetHardMode()
    {
        controller.SetCurrentDifficulty(10.0f);
        Debug.Log("Set to hard mode");
    }

    public void ResetToDefault()
    {
        controller.SetCurrentDifficulty(
            UITemplateIntegrationConstants.DefaultDifficulty
        );
        Debug.Log("Reset to default difficulty");
    }

    public void LogCurrentState()
    {
        Debug.Log($"Difficulty: {controller.GetCurrentDifficulty()}");
        Debug.Log($"Win Streak: {controller.GetWinStreak()}");
        Debug.Log($"Loss Streak: {controller.GetLossStreak()}");
        Debug.Log($"Completion Rate: {controller.GetCompletionRate()}");
        Debug.Log($"Days Away: {controller.GetDaysAway()}");
    }
}
```

---

**Last Updated**: November 4, 2025
**Version**: 2.0.0
**Status**: Complete ✅
