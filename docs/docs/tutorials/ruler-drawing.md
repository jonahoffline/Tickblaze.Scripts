# Writing Ruler Drawing for Tickblaze

This guide walks you through writing the **Ruler Drawing Tool** for Tickblaze. The Ruler is a powerful drawing tool that measures price change, ticks, bars, and time across three user-defined points on the chart.

---

## Prerequisite Knowledge

Before proceeding, here are a few key concepts:

- **Tickblaze API**: Provides tools for creating custom drawings, indicators, and strategies.
- **Drawing Class**: Extend the `Drawing` base class to create custom graphical tools on the chart.
- **Points**: Represents user-defined positions on the chart used for drawing shapes and annotations.

---

## Step-by-Step Guide

### 1. Define the Namespace

Start by declaring a namespace for your drawing:

```csharp
namespace MyScripts.Drawings;
```

**Why Use a Unique Namespace?**

- **Full Type Name**: Tickblaze uses the fully qualified type name (e.g., `MyScripts.Drawings.Ruler`) to uniquely identify your drawing.
- **Avoid Name Conflicts**: A well-structured namespace prevents conflicts with other drawings or scripts.

---

### 2. Create the Ruler Class

Define a new class named `Ruler` and inherit from the `Drawing` base class:

```csharp
public partial class Ruler : Drawing
```

---

### 3. Add Parameters

Parameters let users customize the appearance and behavior of the Ruler tool. Define the parameters for color, thickness, font, and text styling:

```csharp
[Parameter("Line Color", Description = "Color and opacity of the drawn lines")]
public Color LineColor { get; set; } = Color.Gray;

[Parameter("Line Thickness", Description = "Thickness of the drawn lines"), NumericRange(1, 10)]
public int LineThickness { get; set; } = 1;

[Parameter("Lines style", Description = "Line style of the drawn lines")]
public LineStyle LineStyle { get; set; } = LineStyle.Dot;

[Parameter("Text Font", Description = "Font name and size for the text")]
public Font TextFont { get; set; } = new("Arial", 10);

[Parameter("Text Foreground", Description = "Color and opacity of the text")]
public Color TextForeground { get; set; } = Color.Silver;

[Parameter("Text Background", Description = "Color and opacity of the background fill")]
public Color TextBackground { get; set; } = "#33696969";
```

**Breakdown:**

- `[Parameter("Line Color")]`:
  - Declares a user-configurable parameter for customizing the line color.
- `[NumericRange(1, 10)]`:
  - Restricts the `LineThickness` parameter to values between 1 and 10.
- Defaults:
  - `LineColor` is gray.
  - `TextFont` defaults to Arial with size 10.
  - `TextBackground` is a semi-transparent gray color.

---

### 4. Define Points

Define the required points for the Ruler and specify how many are needed:

```csharp
public override int PointsCount => 3;

public IChartPoint PointA => Points[0];
public IChartPoint PointB => Points[1];
public IChartPoint PointC => Points[2];
```

**Breakdown:**

- `PointsCount`:
  - Specifies that the Ruler requires three points.
- `PointA`, `PointB`, `PointC`:
  - Provide user-defined locations on the chart for drawing the ruler.

---

### 5. Implement the Render Logic

The `OnRender` method contains the main logic for rendering the Ruler on the chart. Here’s the implementation:

```csharp
public override void OnRender(IDrawingContext context)
{
    context.DrawPolygon(Points, null, LineColor, LineThickness, LineStyle);

    if (Points.Count < PointsCount)
    {
        return;
    }

    var price = new[] { (double)PointA.Value, (double)PointB.Value };
    var change = price[1] - price[0];
    var ticks = (int)Math.Round(Symbol.RoundToTick(change) / Symbol.TickSize);
    var bars = Chart.GetBarIndexByXCoordinate(PointB.X) - Chart.GetBarIndexByXCoordinate(PointA.X);
    var time = ((DateTime)PointB.Time).Subtract((DateTime)PointA.Time);
    var text = $"Bars:\t{bars}\nTime:\t{time}\nChange:\t{ChartScale.FormatPrice(change)}\nTicks:\t{ticks}";

    var textSize = context.MeasureText(text, TextFont);
    var textMargin = 5;
    var textOrigin = new Point(PointC.X + textMargin, PointC.Y + textMargin);
    var rectPointB = new Point(PointC.X + textSize.Width + textMargin * 2, PointC.Y + textSize.Height + textMargin * 2);

    if (PointB.X > PointC.X)
    {
        textOrigin.X -= textSize.Width + textMargin * 2;
        rectPointB.X = textOrigin.X - textMargin;
    }

    if (PointB.Y > PointC.Y)
    {
        textOrigin.Y -= textSize.Height + textMargin * 2;
        rectPointB.Y = textOrigin.Y - textMargin;
    }

    context.DrawRectangle(PointC, rectPointB, TextBackground, LineColor, LineThickness);
    context.DrawText(textOrigin, text, TextForeground, TextFont);
}
```

**Breakdown:**

1. **Drawing Lines**:
   - `context.DrawPolygon` draws the connecting lines between the three points using the specified color, thickness, and style.

2. **Measurements**:
   - Calculates:
     - **Bars**: The number of bars between `PointA` and `PointB`.
     - **Time**: The time difference between `PointA` and `PointB`.
     - **Change**: The price difference between `PointA` and `PointB`.
     - **Ticks**: Tick-based change derived using `Symbol.RoundToTick`.

3. **Rendering Text**:
   - The calculated measurements are displayed next to `PointC` using `context.DrawText`.
   - `context.MeasureText` calculates the text size to align it correctly within a rectangle background.

---

## Full Source Code

The full source code for the **Ruler Drawing Tool** is available on GitHub. You can review, download, and modify it as needed.

[View the Full Source Code on GitHub](https://github.com/Tickblaze/Tickblaze.Scripts/blob/main/src/Drawings/Ruler.cs)

---

## Conclusion

You’ve successfully created the Ruler drawing tool for Tickblaze. The Ruler measures price, ticks, bars, and time across three points, making it a versatile tool for analyzing chart data. Experiment with parameters and extend it further to fit your trading needs!
