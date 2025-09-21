# 🎮 Dynamic User Difficulty - UITemplate Integration

Complete integration package for Dynamic User Difficulty with UITemplate framework. Provides automatic data persistence, signal handling, and seamless integration with UITemplate's controller pattern.

## 📋 Features

- ✅ **Full UITemplate Integration** - Uses UITemplate's data controllers and local data system
- ✅ **Automatic Signal Handling** - Subscribes to level events and updates difficulty automatically
- ✅ **Complete Data Persistence** - Stores all difficulty data using UITemplate's save system
- ✅ **Provider Implementation** - Implements all provider interfaces for the difficulty module
- ✅ **Session Tracking** - Tracks play sessions, quit types, and time-based metrics
- ✅ **Game Parameter Mapping** - Maps difficulty to concrete game parameters

## 🚀 Quick Start

### 1. Add to Your Project

This package is already added as a submodule in:
```
Packages/com.theone.dynamicuserdifficulty.uitemplateintegration/
```

### 2. Register in VContainer

In your `UITemplateVContainer.cs` or `GameLifetimeScope.cs`:

```csharp
using TheOneStudio.DynamicUserDifficulty.UITemplateIntegration.DI;

protected override void Configure(IContainerBuilder builder)
{
    // One-line registration!
    builder.RegisterDynamicDifficultyUITemplate();

    // That's it! Everything is configured automatically
}
```

### 3. Access Difficulty Anywhere

```csharp
[Inject] private UITemplateDifficultyAdapter difficultyAdapter;

public void StartLevel()
{
    // Get current difficulty (1-10 scale)
    float difficulty = difficultyAdapter.CurrentDifficulty;

    // Get adjusted game parameters
    var parameters = difficultyAdapter.GetAdjustedParameters();

    // Use the parameters
    timeLimit = parameters.TimeLimit;
    hintsAvailable = parameters.HintsAvailable;
}
```

## 📊 Data Stored

The module stores comprehensive difficulty data using UITemplate's local data system:

### Core Difficulty Data
- **Current Difficulty** - Float value between 1-10
- **Win/Loss Streaks** - Consecutive wins and losses
- **Total Statistics** - Total wins, losses, and attempts

### Session Tracking
- **Play Times** - Last play time, session start time
- **Session Duration** - Current and last session duration
- **Quit Types** - Normal, rage quit, or error tracking

### Level Progress
- **Current Level Attempts** - Attempts on current level
- **Completion Time** - Average and last completion times
- **Recent Sessions** - Queue of last 10 play sessions

## 🎯 Automatic Signal Handling

The module automatically subscribes to UITemplate signals:

### Level Signals
- `LevelStartedSignal` - Records level start and resets timer
- `LevelEndedSignal` - Records win/loss and updates difficulty
- `LevelSkippedSignal` - Treats as loss for difficulty calculation

### Application Signals
- `ApplicationQuitSignal` - Records session end and quit type
- `ApplicationPauseSignal` - Handles background/foreground transitions

## 🔧 Game Parameters

The adapter provides adjusted game parameters based on difficulty:

```csharp
public struct GameParameters
{
    public float TimeLimit;        // 120s (easy) -> 60s (hard)
    public int HintsAvailable;     // 5 (easy) -> 1 (hard)
    public float ScoreMultiplier;  // 1x (easy) -> 2x (hard)
    public float EnemySpeed;       // 0.5x (easy) -> 1.5x (hard)
    public float EnemyHealth;      // 50 (easy) -> 150 (hard)
    public float PowerUpFrequency; // 1.5x (easy) -> 0.5x (hard)
}
```

## 📁 Project Structure

```
Runtime/
├── LocalData/
│   └── UITemplateDifficultyData.cs          # Data storage class
├── Controllers/
│   └── UITemplateDifficultyDataController.cs # Controller & providers
├── DI/
│   └── DynamicDifficultyUITemplateModule.cs # VContainer registration
└── UITemplateDifficultyAdapter.cs           # Signal adapter

Editor/
└── DynamicUserDifficultyUITemplate.Editor.asmdef
```

## 🔌 Dependencies

- `com.theone.dynamicuserdifficulty` (v2.0.0) - Core difficulty module
- `UITemplate` - UITemplate framework
- `VContainer` (v1.16.9) - Dependency injection
- `GameFoundation` - Signal bus and utilities

## 🎮 Usage Examples

### Get Current Difficulty
```csharp
float difficulty = difficultyAdapter.CurrentDifficulty;
DifficultyLevel level = difficultyAdapter.GetDifficultyLevel();
```

### Manual Recalculation
```csharp
// Force recalculation (normally automatic)
difficultyAdapter.RecalculateDifficulty();
```

### Reset Difficulty
```csharp
// Reset to default (useful for new game)
difficultyAdapter.ResetDifficulty();
```

### Custom Parameter Mapping
```csharp
// Get parameters and apply custom logic
var parameters = difficultyAdapter.GetAdjustedParameters();

// Apply to your game
enemySpawner.SpawnRate = parameters.EnemySpeed;
player.MaxHints = parameters.HintsAvailable;
scoreManager.Multiplier = parameters.ScoreMultiplier;
```

## 🔄 How It Works

1. **Signal Subscription** - Adapter subscribes to UITemplate game signals
2. **Event Recording** - Controller records wins, losses, and session data
3. **Difficulty Calculation** - Service calculates new difficulty based on data
4. **Data Persistence** - Controller saves using UITemplate's save system
5. **Parameter Mapping** - Adapter maps difficulty to game parameters

## 🛠️ Advanced Configuration

### Custom Modifiers
The system uses all 4 built-in modifiers:
- **Win Streak** - Increases difficulty on consecutive wins
- **Loss Streak** - Decreases difficulty on consecutive losses
- **Time Decay** - Reduces difficulty for returning players
- **Rage Quit** - Adjusts based on quit behavior

### Difficulty Ranges
- **Min**: 1.0 (Easiest)
- **Max**: 10.0 (Hardest)
- **Default**: 3.0 (Normal start)
- **Max Change Per Session**: 2.0

## 📊 Data Access Pattern

Following UITemplate's controller pattern:

```csharp
// ✅ CORRECT - Use the controller
[Inject] private UITemplateDifficultyDataController difficultyController;
float difficulty = difficultyController.GetCurrentDifficulty();

// ❌ WRONG - Never access data directly
var data = difficultyData.CurrentDifficulty; // Don't do this!
```

## 🐛 Debugging

Enable debug logs to see difficulty changes:
```csharp
// Logs are automatically generated for:
- Level starts/ends
- Difficulty changes
- Session tracking
- Signal handling
```

## 📝 License

Part of TheOne Studio's game development framework.

---

Made with ❤️ by TheOne Studio