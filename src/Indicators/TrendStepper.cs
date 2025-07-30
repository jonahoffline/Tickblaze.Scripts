namespace Tickblaze.Scripts.Indicators;

public partial class TrendStepper : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Stair);

	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Step Size", Order = 1), NumericRange(1)]
	public int StepSize { get; set; } = 4;

	[Parameter("Background Opacity", Order = 1, Description = "0=transparent, 100=opaque"), NumericRange(0, 100)]
	public int BackgroundOpacity { get; set; }

	[Parameter("Up Background color", Order = 2, Description = "Color of background of up-trend")]
	public Color UpColor { get; set; } = Color.Green;

	[Parameter("Down Background color", Order = 3, Description = "Color of background of down-trend")]
	public Color DownColor { get; set; } = Color.Red;

	public int LastMovementDir
	{
		get
		{
			Calculate();
			return _trendDir;
		}
	}

	public TrendStepper()
	{
		Name = "Trend Stepper";
		IsOverlay = true;
	}

	private int _trendDir;
	protected override void Calculate(int index)
	{
		if (index == 0)
		{
			Result[index] = Bars.Close[index];
			return;
		}

		var uThresh = Result[index - 1] + Symbol.TickSize * StepSize;
		var lThresh = Result[index - 1] - Symbol.TickSize * StepSize;
		var higherCloseOrOpen = Math.Max(Bars.Close[^1], Bars.Open[^1]);
		var lowerCloseOrOpen = Math.Min(Bars.Close[^1], Bars.Open[^1]);
		var isDoji = higherCloseOrOpen.ApproxCompareTo(lowerCloseOrOpen) == 0;
		var barDir = Bars.Close[^1] > Bars.Open[^1] ? 1 : -1;
		var h = _trendDir == -1 ? higherCloseOrOpen : Bars.High[^1];
		var l = _trendDir == 1 ? lowerCloseOrOpen : Bars.Low[^1];

		Result[index] = Result[index - 1];
		if (h > uThresh && l < lThresh)
		{
			if (isDoji)
			{
				Result[^1] = Result[^2];
			}
			else
			{
				var upDistance = higherCloseOrOpen - Result[^2];
				var downDistance = Result[^2] - lowerCloseOrOpen;
				Result[^1] += upDistance - downDistance;
				Result[^1] = Math.Max(lowerCloseOrOpen, Math.Min(higherCloseOrOpen, Result[^1]));
			}
		}
		else if (h > uThresh)
		{
			var p1 = h - Symbol.TickSize * StepSize;
			var p2 = Bars.High[^2] + Symbol.TickSize * StepSize;
			Result[^1] = Math.Min(p1, p2);
		}
		else if (l < lThresh)
		{
			var p1 = l + Symbol.TickSize * StepSize;
			var p2 = Bars.Low[^2] - Symbol.TickSize * StepSize;
			Result[^1] = Math.Max(p1, p2);
		}
		else
		{
			Result[^1] = Result[^2];
		}

		if (_trendDir == barDir)
			Result[^1] = _trendDir == -1 ? Math.Min(Result[^2], Result[^1]) : Math.Max(Result[^2], Result[^1]);

		if (Result[^1] > Result[^2])
			_trendDir = 1;
		else if (Result[^1] < Result[^2])
			_trendDir = -1;

		if (BackgroundOpacity <= 0 || Bars.Count <= 3 || _trendDir == 0)
			return;

		var bgColor = _trendDir == 1 ? UpColor : DownColor;
		BackgroundColor[^1] = new Color((byte) (255 * BackgroundOpacity / 100), bgColor.R, bgColor.G, bgColor.B);
	}
}

public enum StepMaTrendType
{
	Level,
	Trend
}