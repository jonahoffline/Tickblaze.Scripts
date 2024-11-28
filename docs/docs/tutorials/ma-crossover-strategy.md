# Writing a Moving Average Crossover Strategy for Tickblaze

This guide walks you through writing the **Moving Average Crossover Strategy** for Tickblaze. This strategy buys or sells short when a fast moving average crosses above or below a slow moving average. It also closes any existing positions before entering a new one.

---

## Prerequisite Knowledge

Before proceeding, here are a few key concepts:

- **Tickblaze API**: A robust scripting API to build indicators, strategies, and tools.
- **Strategy Class**: Extend the `Strategy` base class to define custom trading strategies.
- **Order Execution**: Use API methods like `ExecuteMarketOrder` to place trades programmatically.

---

## Step-by-Step Guide

### 1. Define the Namespace

Start by declaring a namespace for your strategy:

```csharp
namespace MyScripts.Strategies;
```

**Why Use a Unique Namespace?**

- **Full Type Name**: Tickblaze uses the fully qualified type name (e.g., `MyScripts.Strategies.MovingAverageCrossover`) to uniquely identify your strategy.
- **Avoid Name Conflicts**: A unique namespace ensures your strategy won’t conflict with others.

---

### 2. Create the Moving Average Crossover Class

Define a new class named `MovingAverageCrossover` and inherit from the `Strategy` base class:

```csharp
public class MovingAverageCrossover : Strategy
```

---

### 3. Add Parameters

Parameters allow users to configure the moving average type and periods used in the strategy. Add the following:

```csharp
[Parameter("MA Type")]
public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Simple;

[Parameter("Fast Period")]
public int FastPeriod { get; set; } = 20;

[Parameter("Slow Period")]
public int SlowPeriod { get; set; } = 50;
```

**Breakdown:**

- `[Parameter("MA Type")]`:
  - Lets users select the type of moving average (e.g., Simple, Exponential).
- `[Parameter("Fast Period")]`:
  - Specifies the period for the fast moving average.
  - Defaults to 20 bars.
- `[Parameter("Slow Period")]`:
  - Specifies the period for the slow moving average.
  - Defaults to 50 bars.

---

### 4. Initialize Moving Averages

In the `Initialize` method, set up the fast and slow moving averages:

```csharp
private MovingAverage _fastMovingAverage, _slowMovingAverage;

protected override void Initialize()
{
    _fastMovingAverage = new MovingAverage(Bars.Close, FastPeriod, MovingAverageType);
    _slowMovingAverage = new MovingAverage(Bars.Close, SlowPeriod, MovingAverageType);
}
```

**Breakdown:**

- `Bars.Close`:
  - Provides the closing prices of the bars, which are used as input for the moving averages.
- `MovingAverage`:
  - Represents a moving average instance. It takes the data series (`Bars.Close`), period, and type as parameters.

---

### 5. Implement the Trading Logic

The `OnBar` method contains the main strategy logic. Tickblaze automatically calls this method whenever a new bar is available.

```csharp
protected override void OnBar(int index)
{
    if (index == 0)
    {
        return;
    }

    var fastLast = _fastMovingAverage[index];
    var fastPrevious = _fastMovingAverage[index - 1];

    var slowLast = _slowMovingAverage[index];
    var slowPrevious = _slowMovingAverage[index - 1];

    var isBullishCrossover = fastLast > slowLast && fastPrevious <= slowPrevious;
    if (isBullishCrossover)
    {
        ClosePosition();
        ExecuteMarketOrder(OrderAction.Buy, 1);
    }

    var isBearishCrossover = fastLast < slowLast && fastPrevious >= slowPrevious;
    if (isBearishCrossover)
    {
        ClosePosition();
        ExecuteMarketOrder(OrderAction.SellShort, 1);
    }
}
```

#### Step-by-Step Logic Inside `OnBar`:

1. **Skip the First Bar**:
   ```csharp
   if (index == 0)
   {
       return;
   }
   ```
   - Ensures there’s enough historical data for comparison.

2. **Fetch Current and Previous Values**:
   ```csharp
   var fastLast = _fastMovingAverage[index];
   var fastPrevious = _fastMovingAverage[index - 1];

   var slowLast = _slowMovingAverage[index];
   var slowPrevious = _slowMovingAverage[index - 1];
   ```
   - Retrieves the current and previous values for both moving averages.

3. **Detect Bullish Crossover**:
   ```csharp
   var isBullishCrossover = fastLast > slowLast && fastPrevious <= slowPrevious;
   if (isBullishCrossover)
   {
       ClosePosition();
       ExecuteMarketOrder(OrderAction.Buy, 1);
   }
   ```
   - A bullish crossover occurs when the fast moving average crosses above the slow moving average.
   - Closes any existing position before entering a **buy order**.

4. **Detect Bearish Crossover**:
   ```csharp
   var isBearishCrossover = fastLast < slowLast && fastPrevious >= slowPrevious;
   if (isBearishCrossover)
   {
       ClosePosition();
       ExecuteMarketOrder(OrderAction.SellShort, 1);
   }
   ```
   - A bearish crossover occurs when the fast moving average crosses below the slow moving average.
   - Closes any existing position before entering a **sell short order**.

---

## Conclusion

You’ve successfully created a **Moving Average Crossover Strategy** for Tickblaze. This updated logic ensures existing positions are closed before new ones are opened, making the strategy cleaner and more precise. Experiment with different moving average types and periods to optimize performance. Happy coding!
