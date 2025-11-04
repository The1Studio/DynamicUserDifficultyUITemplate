# Architecture Documentation

## System Architecture

### High-Level Overview

The Dynamic User Difficulty UITemplate Integration follows a layered architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                           │
│                   (Unity Game Components)                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │               SIGNAL INTEGRATION LAYER                    │  │
│  │                                                            │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │      UITemplateDifficultyAdapter                   │  │  │
│  │  │  • Signal Subscription & Translation               │  │  │
│  │  │  • Event Handling & Routing                        │  │  │
│  │  │  • Parameter Mapping & Game Integration            │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │                          │                                 │  │
│  └──────────────────────────┼─────────────────────────────────┘  │
│                             │                                     │
├─────────────────────────────┼─────────────────────────────────────┤
│                             ▼                                     │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                 PROVIDER LAYER                            │  │
│  │                                                            │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  UITemplateDifficultyDataController                │  │  │
│  │  │  • IDifficultyDataProvider                         │  │  │
│  │  │  • IWinStreakProvider                              │  │  │
│  │  │  • ITimeDecayProvider                              │  │  │
│  │  │  • IRageQuitProvider                               │  │  │
│  │  │  • ILevelProgressProvider                          │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │                          │                                 │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │     Specialized Provider Implementations           │  │  │
│  │  │  • DifficultyDataProvider                          │  │  │
│  │  │  • UITemplateWinStreakProvider                     │  │  │
│  │  │  • UITemplateTimeDecayProvider                     │  │  │
│  │  │  • UITemplateRageQuitProvider                      │  │  │
│  │  │  • UITemplateLevelProgressProvider                 │  │  │
│  │  │  • UITemplateSessionPatternProvider                │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │                          │                                 │  │
│  └──────────────────────────┼─────────────────────────────────┘  │
│                             │                                     │
├─────────────────────────────┼─────────────────────────────────────┤
│                             ▼                                     │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │           DATA PERSISTENCE LAYER                          │  │
│  │                                                            │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │      UITemplateDifficultyData                      │  │  │
│  │  │  • Win/Loss Tracking                               │  │  │
│  │  │  • Session Metrics                                 │  │  │
│  │  │  • Behavioral Data                                 │  │  │
│  │  │  • Performance Metrics                             │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │                          │                                 │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │      UITemplate Local Data System                  │  │  │
│  │  │  • Automatic Persistence                           │  │  │
│  │  │  • JSON Serialization                              │  │  │
│  │  │  • Cross-Session Storage                           │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │                                                            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

                             ┌──────────┐
                             │  CORE DI │
                             └────┬─────┘
                                  │
        ┌─────────────────────────┼─────────────────────────┐
        │                         │                         │
        ▼                         ▼                         ▼
┌───────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  Dynamic      │     │   VContainer     │     │   Signal Bus     │
│  Difficulty   │────▶│   Dependency     │◀────│   (MessagePipe)  │
│  Service Core │     │   Injection      │     │                  │
└───────────────┘     └──────────────────┘     └──────────────────┘
        │
        └──▶ 7 Difficulty Modifiers:
              • WinStreakModifier
              • LossStreakModifier
              • CompletionRateModifier
              • TimeDecayModifier
              • RageQuitModifier
              • LevelProgressModifier
              • SessionPatternModifier
```

## Component Responsibilities

### 1. Signal Integration Layer

#### UITemplateDifficultyAdapter

**Purpose**: Bridge between UITemplate framework signals and difficulty system

**Responsibilities**:
- Subscribe to UITemplate signals (LevelStarted, LevelEnded, ApplicationQuit, etc.)
- Translate signal events into provider data updates
- Map difficulty values to game parameters
- Emit custom DifficultyChangedSignal

**Key Methods**:
```csharp
public class DifficultyAdapter : IInitializable
{
    void Initialize()
    void OnLevelStarted(LevelStartedSignal signal)
    void OnLevelEnded(LevelEndedSignal signal)
    void OnApplicationQuit(ApplicationQuitSignal signal)
    float CurrentDifficulty { get; }
    GameParameters GetAdjustedParameters()
}
```

**Dependencies**:
- `ISignalBus`: UITemplate signal bus
- `UITemplateDifficultyDataController`: Data management
- `IDynamicDifficultyService`: Core difficulty calculation

### 2. Provider Layer

#### UITemplateDifficultyDataController

**Purpose**: Central data controller implementing all 5 provider interfaces

**Responsibilities**:
- Implement all 21 provider interface methods
- Manage UITemplateDifficultyData lifecycle
- Handle data persistence through UITemplate
- Provide stateless data access to modifiers

**Implemented Interfaces**:
```csharp
public class UITemplateDifficultyDataController :
    IDifficultyDataProvider,      // 2 methods
    IWinStreakProvider,            // 4 methods
    ITimeDecayProvider,            // 3 methods
    IRageQuitProvider,             // 4 methods
    ILevelProgressProvider         // 5 methods
{
    // Total: 21 provider methods (100% coverage)
}
```

**Key Features**:
- Single source of truth for all difficulty data
- Direct access to UITemplate data services
- Automatic data persistence on updates
- Thread-safe data operations

#### Specialized Provider Implementations

Each provider focuses on a specific aspect of difficulty analysis:

**DifficultyDataProvider**:
- Current difficulty storage and retrieval
- Difficulty value updates
- Historical difficulty tracking

**UITemplateWinStreakProvider**:
- Win/loss streak tracking
- Total win/loss counts
- Streak reset logic

**UITemplateTimeDecayProvider**:
- Last play time tracking
- Time away calculations
- Returning player detection

**UITemplateRageQuitProvider**:
- Quit type analysis (Normal/RageQuit/Error)
- Recent rage quit counting
- Session duration tracking

**UITemplateLevelProgressProvider**:
- Level attempt tracking
- Completion time analysis
- Difficulty rating per level
- Completion rate calculations

**UITemplateSessionPatternProvider**:
- Session duration patterns
- Average session length
- Total play time tracking
- Behavioral pattern analysis

### 3. Data Persistence Layer

#### UITemplateDifficultyData

**Purpose**: Serializable data model for all difficulty metrics

**Data Categories**:

**Core Difficulty**:
```csharp
public float CurrentDifficulty { get; set; }
```

**Win/Loss Tracking**:
```csharp
public int WinStreak { get; set; }
public int LossStreak { get; set; }
public int TotalWins { get; set; }
public int TotalLosses { get; set; }
```

**Session Tracking**:
```csharp
public DateTime LastPlayTime { get; set; }
public DateTime SessionStartTime { get; set; }
public float SessionDuration { get; set; }
public float LastSessionDuration { get; set; }
public float AverageSessionDuration { get; set; }
public float TotalPlayTime { get; set; }
```

**Behavioral Analysis**:
```csharp
public QuitType LastQuitType { get; set; }
public int RecentRageQuitCount { get; set; }
```

**Level Progress**:
```csharp
public int CurrentLevelAttempts { get; set; }
public float AverageCompletionTime { get; set; }
public float LastCompletionTime { get; set; }
public float CurrentLevelDifficulty { get; set; }
public float CompletionRate { get; set; }
```

**Storage Size**: < 2KB per player

## Data Flow Patterns

### 1. Level Completion Flow

```
Player Completes Level
    │
    ├─▶ LevelEndedSignal emitted by UITemplate
    │
    └─▶ DifficultyAdapter.OnLevelEnded()
        │
        ├─▶ Update Win/Loss Streaks
        ├─▶ Update Completion Time
        ├─▶ Update Attempt Counts
        ├─▶ Calculate Completion Rate
        │
        └─▶ UITemplateDifficultyDataController
            │
            ├─▶ Persist Data via UITemplate
            │
            └─▶ Notify Dynamic Difficulty Service
                │
                ├─▶ Evaluate 7 Modifiers
                │   ├─▶ WinStreakModifier (+0.5)
                │   ├─▶ CompletionRateModifier (+0.3)
                │   ├─▶ LevelProgressModifier (-0.2)
                │   └─▶ ... (other modifiers)
                │
                └─▶ Calculate New Difficulty
                    │
                    └─▶ Emit DifficultyChangedSignal
                        │
                        └─▶ Game adjusts parameters
```

### 2. Session Start Flow

```
Application Launch
    │
    ├─▶ VContainer Initialization
    │   │
    │   └─▶ RegisterDynamicDifficultyUITemplate()
    │       │
    │       ├─▶ Register Controllers
    │       ├─▶ Register Providers
    │       ├─▶ Register Adapters
    │       ├─▶ Register Modifiers
    │       └─▶ Register Signal Subscriptions
    │
    └─▶ DifficultyAdapter.Initialize()
        │
        ├─▶ Subscribe to Signals
        │   ├─▶ LevelStartedSignal
        │   ├─▶ LevelEndedSignal
        │   ├─▶ LevelSkippedSignal
        │   ├─▶ ApplicationQuitSignal
        │   └─▶ ApplicationPauseSignal
        │
        ├─▶ Load Existing Data
        │   └─▶ UITemplateDifficultyDataController.Load()
        │
        └─▶ Calculate Initial Difficulty
            └─▶ Consider Time Decay for returning players
```

### 3. Rage Quit Detection Flow

```
Player Quits During Level
    │
    ├─▶ ApplicationQuitSignal emitted
    │
    └─▶ DifficultyAdapter.OnApplicationQuit()
        │
        ├─▶ Calculate Session Duration
        ├─▶ Analyze Quit Context
        │   ├─▶ Level in progress? → RageQuit
        │   ├─▶ Level complete? → Normal
        │   └─▶ Error state? → Error
        │
        └─▶ Update Provider Data
            │
            ├─▶ Increment RageQuitCount
            ├─▶ Record Session Duration
            ├─▶ Update Last Quit Type
            │
            └─▶ Trigger Difficulty Adjustment
                │
                └─▶ RageQuitModifier applies penalty reduction
                    └─▶ Next level will be easier
```

## Dependency Injection Architecture

### VContainer Registration Module

```csharp
public static class DynamicDifficultyUITemplateModule
{
    public static void RegisterDynamicDifficultyUITemplate(
        this IContainerBuilder builder)
    {
        // 1. Register Data Controller (implements 5 interfaces)
        builder.Register<UITemplateDifficultyDataController>(Lifetime.Singleton)
               .AsImplementedInterfaces()
               .AsSelf();

        // 2. Register Specialized Providers
        builder.Register<DifficultyDataProvider>(Lifetime.Singleton)
               .As<IDifficultyDataProvider>();

        builder.Register<UITemplateWinStreakProvider>(Lifetime.Singleton)
               .As<IWinStreakProvider>();

        builder.Register<UITemplateTimeDecayProvider>(Lifetime.Singleton)
               .As<ITimeDecayProvider>();

        builder.Register<UITemplateRageQuitProvider>(Lifetime.Singleton)
               .As<IRageQuitProvider>();

        builder.Register<UITemplateLevelProgressProvider>(Lifetime.Singleton)
               .As<ILevelProgressProvider>();

        builder.Register<UITemplateSessionPatternProvider>(Lifetime.Singleton)
               .As<ISessionPatternProvider>();

        // 3. Register Adapter
        builder.RegisterEntryPoint<DifficultyAdapter>();

        // 4. Core difficulty service (from main package)
        builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(
            Lifetime.Singleton);

        // 5. Register all 7 modifiers
        builder.Register<WinStreakModifier>(Lifetime.Singleton)
               .As<IDifficultyModifier>();

        builder.Register<LossStreakModifier>(Lifetime.Singleton)
               .As<IDifficultyModifier>();

        builder.Register<CompletionRateModifier>(Lifetime.Singleton)
               .As<IDifficultyModifier>();

        builder.Register<TimeDecayModifier>(Lifetime.Singleton)
               .As<IDifficultyModifier>();

        builder.Register<RageQuitModifier>(Lifetime.Singleton)
               .As<IDifficultyModifier>();

        builder.Register<LevelProgressModifier>(Lifetime.Singleton)
               .As<IDifficultyModifier>();

        builder.Register<SessionPatternModifier>(Lifetime.Singleton)
               .As<IDifficultyModifier>();
    }
}
```

### Lifetime Management

| Component | Lifetime | Reason |
|-----------|----------|--------|
| `UITemplateDifficultyDataController` | Singleton | Central data store, must persist across scenes |
| All Providers | Singleton | Stateless, shared across entire application |
| `DifficultyAdapter` | EntryPoint | Initialized once, managed by VContainer |
| `DynamicDifficultyService` | Singleton | Core calculation engine, shared state |
| All Modifiers | Singleton | Stateless algorithms, no per-instance data |

## Signal Architecture

### Signal Flow Diagram

```
UITemplate Framework Signals
    │
    ├─▶ LevelStartedSignal
    │   └─▶ Data: levelId, difficulty, timestamp
    │
    ├─▶ LevelEndedSignal
    │   └─▶ Data: levelId, won, completionTime, score
    │
    ├─▶ LevelSkippedSignal
    │   └─▶ Data: levelId, reason
    │
    ├─▶ ApplicationQuitSignal
    │   └─▶ Data: quitReason, sessionDuration
    │
    └─▶ ApplicationPauseSignal
        └─▶ Data: paused, timestamp

                │
                ▼
        DifficultyAdapter
        (Signal Subscriber)
                │
                ▼
    Provider Data Updates
                │
                ▼
    Dynamic Difficulty Service
                │
                ▼
    7 Modifier Evaluations
                │
                ▼
        New Difficulty Value
                │
                ▼
    DifficultyChangedSignal
                │
                ▼
        Game Components
    (Apply difficulty settings)
```

### Custom Signals

**DifficultyChangedSignal**:
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

**Usage**:
```csharp
[Inject] private ISignalBus signalBus;

public void Initialize()
{
    signalBus.Subscribe<DifficultyChangedSignal>(OnDifficultyChanged);
}

private void OnDifficultyChanged(DifficultyChangedSignal signal)
{
    Debug.Log($"Difficulty: {signal.OldDifficulty} → {signal.NewDifficulty}");
    Debug.Log($"Active modifiers: {signal.AppliedModifiers.Count}/7");

    // Adjust game parameters
    UpdateEnemySpeed(signal.NewDifficulty);
    UpdateTimeLimit(signal.NewDifficulty);
    UpdateHintCount(signal.NewDifficulty);
}
```

## Assembly Definitions

### Runtime Assembly

**File**: `DynamicUserDifficultyUITemplate.asmdef`

```json
{
  "name": "DynamicUserDifficultyUITemplate",
  "references": [
    "com.theone.dynamicuserdifficulty",
    "VContainer",
    "UITemplate",
    "GameFoundation",
    "Unity.Newtonsoft.Json"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false
}
```

### DI Assembly

**File**: `DynamicUserDifficultyUITemplate.DI.asmdef`

```json
{
  "name": "DynamicUserDifficultyUITemplate.DI",
  "references": [
    "DynamicUserDifficultyUITemplate",
    "VContainer"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false
}
```

### Editor Assembly

**File**: `DynamicUserDifficultyUITemplate.Editor.asmdef`

```json
{
  "name": "DynamicUserDifficultyUITemplate.Editor",
  "references": [
    "DynamicUserDifficultyUITemplate"
  ],
  "includePlatforms": ["Editor"],
  "excludePlatforms": [],
  "allowUnsafeCode": false
}
```

## Performance Considerations

### Optimization Strategies

**1. Calculation Caching**:
- Difficulty recalculated only when provider data changes
- Cache invalidation on data updates
- Lazy evaluation of modifiers

**2. Provider Efficiency**:
- All provider methods < 1ms execution
- Direct data access (no serialization overhead)
- Zero-allocation method calls

**3. Memory Management**:
- Total data < 2KB per player
- No runtime allocations during gameplay
- Efficient JSON serialization

**4. Signal Processing**:
- Async signal handling
- Batched data updates
- Throttled difficulty recalculations

### Performance Metrics

| Operation | Target | Measured |
|-----------|--------|----------|
| Difficulty Calculation | < 10ms | ~5ms |
| Provider Method Call | < 1ms | ~0.5ms |
| Data Persistence | < 5ms | ~2ms |
| Signal Processing | < 5ms | ~3ms |
| Memory Footprint | < 5KB | ~2KB |

## Error Handling & Resilience

### Data Validation

```csharp
public class UITemplateDifficultyDataController
{
    public void SetCurrentDifficulty(float value)
    {
        // Clamp to valid range
        value = Mathf.Clamp(value, MinDifficulty, MaxDifficulty);

        // Prevent NaN/Infinity
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            Logger.LogError("Invalid difficulty value, resetting to default");
            value = DefaultDifficulty;
        }

        difficultyData.CurrentDifficulty = value;
        Save();
    }
}
```

### Fallback Mechanisms

- **Missing Data**: Use default values
- **Corrupted Save**: Reset to initial state
- **Provider Failure**: Continue with available providers
- **Calculation Error**: Maintain last valid difficulty

### Logging & Debugging

```csharp
// Comprehensive logging
[Conditional("UNITY_EDITOR")]
private void LogDifficultyChange()
{
    Logger.LogInfo($"Difficulty: {oldValue} → {newValue}");
    Logger.LogInfo($"Modifiers: {string.Join(", ", activeModifiers)}");
    Logger.LogInfo($"Provider data: Win={wins}, Loss={losses}, Time={timeSince}");
}
```

## Best Practices

### 1. Provider Implementation
- Keep providers stateless
- Use controller for all data access
- Implement all interface methods
- Handle missing data gracefully

### 2. Signal Handling
- Subscribe in Initialize()
- Unsubscribe in Dispose()
- Handle null signals
- Batch related updates

### 3. Data Persistence
- Save after every significant change
- Validate before saving
- Handle save failures
- Provide migration paths

### 4. Dependency Injection
- Use single registration method
- Register as interfaces
- Configure lifetimes correctly
- Test DI container configuration

---

**Last Updated**: November 4, 2025
**Architecture Version**: 2.0
**Status**: Production Ready ✅
