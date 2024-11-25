# Writing Aroon Indicator for Tickblaze

This guide walks you through writing the **Aroon Indicator (ARN)** for Tickblaze. We’ll explain the **Tickblaze API** syntax in detail to help new developers understand and build their custom indicators.

---

## Prerequisite Knowledge

Before proceeding, here are a few key concepts:

- **Tickblaze API**: A powerful scripting API to build indicators, strategies, and other tools.
- **Bars**: Represents a collection of price bars (e.g., candlesticks) with attributes like `High`, `Low`, and `Close`.
- **Indicators**: Extend the `Indicator` base class, allowing you to define parameters, calculations, and plots.

---

## Step-by-Step Guide

### 1. Define the Namespace

Start by declaring a namespace for your indicator:

```csharp
namespace MyScripts.Indicators;
```

**Why Use a Unique Namespace?**

- **Full Type Name**: Tickblaze uses the fully qualified type name (e.g., `MyScripts.Indicators.Aroon`) to uniquely identify your script.
- **Avoid Name Conflicts**: A well-structured namespace prevents conflicts with other indicators or scripts in the Tickblaze ecosystem.

### 2. Create the Aroon Class

Define a new class named `Aroon` and inherit from the `Indicator` base class. Add the `partial` modifier to enable automatic constructor generation.

```cs
public partial class Aroon : Indicator
```

**Why Use the `partial` modifier?**

- The `partial` modifier signals to the Tickblaze source generator that this class can have its code automatically extended. Specifically, constructors based on parameters (`[Parameter]`) will be generated for you.

### 3. Add Parameters

Parameters let users configure the indicator. Add a **period parameter**:

```cs
[Parameter("Period"), NumericRange(1, int.MaxValue)]
public int Period { get; set; } = 14;
```

**Breakdown:**

- `[Parameter("Period")]`:
  - Declares a user-configurable parameter named "Period" in the UI.
  - Users can adjust this value when adding the indicator to a chart.
- `[NumericRange(1, int.MaxValue)]`:
  - Ensures the parameter value is always between 1 and the maximum possible integer, preventing invalid inputs.
- `public int Period`:
  - `int` specifies this parameter is an integer.
  - The default value is set to `14`.

### 4. Define Output Plots

Plots represent lines or values drawn on the chart. Add two plots: **Aroon Up** and **Aroon Down**.

```cs
[Plot("Aroon Up")]
public PlotSeries Up { get; set; } = new(Color.Orange, PlotStyle.Line);

[Plot("Aroon Down")]
public PlotSeries Down { get; set; } = new(Color.Blue, PlotStyle.Line);
```

**Breakdown:**

- `[Plot("Aroon Up")]`:
  - Registers a plot named "Aroon Up".
  - Displays this line in the chart’s legend.
- `PlotSeries`:
  - Represents the data series plotted on the chart.
  - Accepts parameters like `Color` and `PlotStyle`.
- `new(Color.Orange, PlotStyle.Line)`:
  - Sets the line's color to orange and style to a solid line.

### 5. Add the Constructor

Initialize the indicator with default metadata:

```cs
public Aroon()
{
    Name = "Aroon";
    ShortName = "ARN";
    IsOverlay = false;
    IsPercentage = true;
}
```

**Breakdown:**

- `Name`:
  - The full name of the indicator displayed in the UI.
- `ShortName`:
  - A short abbreviation for compact views.
- `IsOverlay`:
  - Indicates whether the indicator will overlay the bars or be placed in a new chart panel.
- `IsPercentage`:
  - Indicates that the plot values are percentages (0-100).

### 6. Implement the Calculation Logic

The `Calculate` method contains the main logic for the indicator. Tickblaze automatically calls this method for each bar on the chart.

```cs
protected override void Calculate(int index)
```

#### Parameters:

- `int index`:
  - Represents the current bar being calculated.
  - You can access the price data for this bar using `Bars`.

#### Step-by-Step Logic Inside `Calculate`:

**1. Skip Bars Without Enough Data**
   ```cs
   if (index < Period)
   {
      return;
   }
   ```
   - Ensures the logic only runs when there are enough bars to calculate the Aroon values.

**2. Initialize Variables**
   ```cs
   var currentHigh = Bars.High[index];
   var currentLow = Bars.Low[index];
   var barsSinceHigh = 0;
   var barsSinceLow = 0;
   ```
   - `Bars.High[index]`: Fetches the high price of the current bar.
   - `Bars.Low[index]`: Fetches the low price of the current bar.
   - `barsSinceHigh` and `barsSinceLow`: Track how many bars ago the highest and lowest prices occurred.

**3. Loop Through the Period**
   ```cs
   for (var i = 0; i < Period; i++)
   {
       if (Bars.High[index - i] >= currentHigh)
       {
           currentHigh = Bars.High[index - i];
           barsSinceHigh = i;
       }

       if (Bars.Low[index - i] <= currentLow)
       {
           currentLow = Bars.Low[index - i];
           barsSinceLow = i;
       }
   }
   ```
   - **Loop**: Iterates through the last `Period` bars.
   - **Update High/Low**: If a new high or low is found, update `currentHigh` or `currentLow` and record the number of bars since it occurred.

**4. Calculate Aroon Up and Down**
   ```cs
   Up[index] = (Period - barsSinceHigh - 1) * 100.0 / (Period - 1);
   Down[index] = (Period - barsSinceLow - 1) * 100.0 / (Period - 1);
   ```
   - **Aroon Up**: The percentage of time since the most recent high.
   - **Aroon Down**: The percentage of time since the most recent low.
   - Multiply by `100.0` to normalize the values to percentages.

---

## Full Source Code

For your convenience, the complete source code of the **Aroon Indicator** is available on GitHub. You can review it, download it, or fork the repository to modify and enhance it further.

[View the Full Source Code on GitHub](https://github.com/Tickblaze/Tickblaze.Scripts/blob/main/src/Indicators/Aroon.cs)
