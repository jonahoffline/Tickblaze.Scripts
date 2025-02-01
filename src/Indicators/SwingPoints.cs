using System.Diagnostics.CodeAnalysis;

namespace Tickblaze.Scripts.Indicators;

public partial class SwingPoints : Indicator
{
	public SwingPoints()
	{
		IsOverlay = true;

		ShortName = "SP";

		Name = "Swing Points";
	}

	private int _currentIndex;

	[AllowNull]
	private Series<double> _swingLows;

	[AllowNull]
	private Series<double> _swingHighs;

	[NumericRange(MinValue = 1, MaxValue = 256)]
	[Parameter("Swing Strength", Description = "Number of bars used to identify a swing high or low")]
	public int SwingStrength { get; set; } = 5;

	[NumericRange(MinValue = 1)]
	[Parameter("Swing Dot Size", Description = "Size of the swing dots")]
	public int SwingDotSize { get; set; } = 6;

	[Parameter("Swing High Dot Color", Description = "Color of the swing high dots")]
	public Color SwingHighColor { get; set; } = Color.Cyan;

	[Parameter("Swing Low Dot Color", Description = "Color of the swing low dots")]
	public Color SwingLowColor { get; set; } = Color.Yellow;

	protected override void Initialize()
	{
		_swingLows = [];

		_swingHighs = [];

		_currentIndex = 2 * SwingStrength;
	}

	protected override void Calculate(int barIndex)
	{
		_swingLows[barIndex] = _swingHighs[barIndex] = double.NaN;

		if (_currentIndex >= barIndex)
		{
			return;
		}

		_currentIndex = barIndex;

		var isSwingLow = true;
		var isSwingHigh = true;

		var middleBarIndex = barIndex - SwingStrength - 1;
		var middleLow = Bars.Low[middleBarIndex];
		var middleHigh = Bars.High[middleBarIndex];

		var startBarIndex = middleBarIndex - SwingStrength;
		var endBarIndex = middleBarIndex + SwingStrength;

		for (var currentBarIndex = startBarIndex; currentBarIndex <= endBarIndex; currentBarIndex++)
		{
			if (currentBarIndex.Equals(middleBarIndex))
			{
				continue;
			}

			var currentLow = Bars.Low[currentBarIndex];
			var compareLow = middleLow.ApproxCompareTo(currentLow);

			var currentHigh = Bars.High[currentBarIndex];
			var compareHigh = middleHigh.ApproxCompareTo(currentHigh);

			var isEqualityAllowed = currentBarIndex >= middleBarIndex;

			isSwingLow &= compareLow < 0 || isEqualityAllowed && compareLow is 0;
			isSwingHigh &= compareHigh > 0 || isEqualityAllowed && compareHigh is 0;
		}

		UpdateSwingHighs(isSwingHigh, middleBarIndex);

		UpdateSwingLows(isSwingLow, middleBarIndex);
	}

	private void UpdateSwingHighs(bool isSwingHigh, int middleBarIndex)
	{
		var isSwingHighBreak = false;

		var swingHigh = _swingHighs[middleBarIndex - 1];

		var endBarIndex = middleBarIndex + SwingStrength;

		var startBarIndex = endBarIndex;

		if (isSwingHigh)
		{
			swingHigh = Bars.High[middleBarIndex];

			startBarIndex = middleBarIndex;
		}

		for (var currentBarIndex = startBarIndex; currentBarIndex <= endBarIndex; currentBarIndex++)
		{
			var currentBarHigh = Bars.High[currentBarIndex];

			isSwingHighBreak |= currentBarHigh.ApproxGreaterThan(swingHigh);

			if (!isSwingHighBreak)
			{
				_swingHighs[currentBarIndex] = swingHigh;
			}
		}
	}

	private void UpdateSwingLows(bool isSwingLow, int middleBarIndex)
	{
		var isSwingLowBreak = false;

		var swingLow = _swingLows[middleBarIndex - 1];

		var endBarIndex = middleBarIndex + SwingStrength;

		var startBarIndex = endBarIndex;

		if (isSwingLow)
		{
			swingLow = Bars.Low[middleBarIndex];

			startBarIndex = middleBarIndex;
		}

		for (var currentBarIndex = startBarIndex; currentBarIndex <= endBarIndex; currentBarIndex++)
		{
			var currentBarLow = Bars.Low[currentBarIndex];

			isSwingLowBreak |= currentBarLow.ApproxLessThan(swingLow);

			if (!isSwingLowBreak)
			{
				_swingLows[currentBarIndex] = swingLow;
			}
		}
	}

	public override void OnRender(IDrawingContext drawingContext)
	{
		var startBarIndex = Chart.FirstVisibleBarIndex;
		var endBarIndex = Math.Min(Bars.Count - 2, Chart.LastVisibleBarIndex);

		for (var barIndex = startBarIndex; barIndex <= endBarIndex; barIndex++)
		{
			var swingLow = _swingLows[barIndex];
			var swingHigh = _swingHighs[barIndex];

			DrawSwingDot(drawingContext, barIndex, swingLow, SwingLowColor);
			DrawSwingDot(drawingContext, barIndex, swingHigh, SwingHighColor);
		}
	}

	private void DrawSwingDot(IDrawingContext drawingContext, int barIndex, double price, Color color)
	{
		if (double.IsNaN(price))
		{
			return;
		}

		var dotRadius = SwingDotSize / 2.0;

		var point = new Point
		{
			X = Chart.GetXCoordinateByBarIndex(barIndex),
			Y = ChartScale.GetYCoordinateByValue(price),
		};

		drawingContext.DrawEllipse(point, dotRadius, dotRadius, color);
	}
}
