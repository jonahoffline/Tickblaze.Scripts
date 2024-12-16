# Custom Rendering

In this tutorial, we will learn how to use custom rendering in Tickblaze.Scripts.Api. Custom rendering allows you to draw custom graphics on the chart during each render cycle.

## Creating a Custom Drawing

Let's create a custom drawing tool called `ClosePriceLine`. This tool will draw a line connecting the close prices of the bars within the selected range.

### Step 1: Define the Drawing Class

First, define the `ClosePriceLine` class that inherits from the `Drawing` class.

```cs
namespace Tickblaze.Scripts.Tests;

public class ClosePriceLine : Drawing
{
	[Parameter("Line Color", Description = "Color of the custom line")]
	public Color LineColor { get; set; } = Color.Blue;

	[Parameter("Line Thickness", Description = "Thickness of the custom line"), NumericRange(1)]
	public int LineThickness { get; set; } = 2;

	[Parameter("Line Style", Description = "Style of the custom line")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => 2;

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		var barIndex = Chart.GetBarIndexByXCoordinate(Points[index].X);
		var bar = Bars[barIndex];

		Points[index].Value = bar.Close;
	}

	public override void OnRender(IDrawingContext context)
	{
		if (Points.Count < PointsCount)
		{
			return;
		}

		var points = new List<Point>();

		var fromBarIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		var toBarIndex = Chart.GetBarIndexByXCoordinate(Points[1].X);

		if (fromBarIndex > toBarIndex)
		{
			(fromBarIndex, toBarIndex) = (toBarIndex, fromBarIndex);
		}

		for (var barIndex = fromBarIndex; barIndex <= toBarIndex; barIndex++)
		{
			var point = new Point()
			{
				X = Chart.GetXCoordinateByBarIndex(barIndex),
				Y = ChartScale.GetYCoordinateByValue(Bars[barIndex].Close)
			};

			points.Add(point);
		}

		context.DrawPolygon(points, null, LineColor, LineThickness, LineStyle);
	}
}

```

### Step 2: Implement the OnRender Method

The `OnRender` method is where you define the custom rendering logic. This method is called during each render cycle, allowing you to draw custom graphics on the chart.

In the `ClosePriceLine` class, the `OnRender` method draws the close price line between two anchor points.

### Step 3: Add the Drawing Tool to the Chart

To use the custom drawing tool, add it to the chart in the Tickblaze platform. You can do this by selecting the `ClosePriceLine` tool from the drawing tools menu and placing it on the chart.

## Applying Custom Rendering to Other Script Types

The `OnRender` method can be overridden in various script types such as Indicators, Strategies, and Drawings. This allows you to apply custom rendering logic across different types of scripts in Tickblaze.

### Example: Custom Indicator

Let's create a custom indicator that uses the `OnRender` method to draw custom graphics on the chart.

```cs
namespace Tickblaze.Scripts.Indicators;

public partial class ClosePriceLine : Indicator
{
	[Parameter("Line Color", Description = "Color of the custom line")]
	public Color LineColor { get; set; } = Color.Blue;

	[Parameter("Line Thickness", Description = "Thickness of the custom line"), NumericRange(1)]
	public int LineThickness { get; set; } = 2;

	[Parameter("Line Style", Description = "Style of the custom line")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	public override void OnRender(IDrawingContext context)
	{
		var points = new List<Point>();

		for (var barIndex = Chart.FirstVisibleBarIndex; barIndex <= Chart.LastVisibleBarIndex; barIndex++)
		{
			var point = new Point()
			{
				X = Chart.GetXCoordinateByBarIndex(barIndex),
				Y = ChartScale.GetYCoordinateByValue(Bars[barIndex].Close)
			};

			points.Add(point);
		}

		context.DrawPolygon(points, null, LineColor, LineThickness, LineStyle);
	}
}
```

### Example: Custom Strategy

Similarly, you can create a custom strategy that uses the `OnRender` method to draw custom graphics on the chart.

```cs
namespace Tickblaze.Scripts.Strategies;

public class ClosePriceLine : Strategy
{
	[Parameter("Line Color", Description = "Color of the custom line")]
	public Color LineColor { get; set; } = Color.Blue;

	[Parameter("Line Thickness", Description = "Thickness of the custom line"), NumericRange(1)]
	public int LineThickness { get; set; } = 2;

	[Parameter("Line Style", Description = "Style of the custom line")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	public override void OnRender(IDrawingContext context)
	{
		var points = new List<Point>();

		for (var barIndex = Chart.FirstVisibleBarIndex; barIndex <= Chart.LastVisibleBarIndex; barIndex++)
		{
			var point = new Point()
			{
				X = Chart.GetXCoordinateByBarIndex(barIndex),
				Y = ChartScale.GetYCoordinateByValue(Bars[barIndex].Close)
			};

			points.Add(point);
		}

		context.DrawPolygon(points, null, LineColor, LineThickness, LineStyle);
	}
}
```

### Conclusion

Custom rendering in Tickblaze.Scripts.Api is not limited to drawing tools. By overriding the `OnRender` method, you can apply custom rendering logic to Indicators, Strategies, and other script types. This flexibility allows you to create powerful and visually rich custom scripts for your trading needs.































































