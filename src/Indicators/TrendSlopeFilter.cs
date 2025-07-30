namespace Tickblaze.Scripts.Indicators;

public partial class TrendSlopeFilter : Indicator, ITrendSlopeFilter
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; } = null!;

	public MovingAverageType SlopeMaType { get; set; }
	public int SlopeMaPeriod { get; set; }
	public int SlopePeriod { get; set; }
	public SlopeRuleType SlopeRule { get; set; }
	public double SlopeMaximum { get; set; }
	public double SlopeMinimum { get; set; }
	public Stroke SlopeStroke { get; set; }
	public Stroke SlopeZeroLevelStroke { get; set; }
	public Stroke SlopeThresholdLevelStroke { get; set; }
	public Color SlopeShadingColor { get; set; }

	[Plot("Slope")]
	public PlotSeries Slope { get; set; } = new PlotSeries(Color.Yellow) { IsEditorBrowsable = false };

	public ISeries<bool> IsTrending => _isTrending;

	private Series<bool> _isTrending = null!;

	private MovingAverage _movingAverage = null!;
	private LinearRegressionSlope _linearRegressionSlope = null!;

	public TrendSlopeFilter()
	{
		((ITrendSlopeFilter) this).SetDefaults();
	}

	protected override void Initialize()
	{
		Slope.Stroke = SlopeStroke;

		_isTrending = new Series<bool>();
		_movingAverage = new MovingAverage(Source, SlopeMaPeriod, SlopeMaType);
		_linearRegressionSlope = new LinearRegressionSlope(_movingAverage.Result, SlopePeriod);
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		((ITrendSlopeFilter)this).AdjustParameters(parameters);
		return parameters;
	}

	protected override void Calculate(int index)
	{
		var slope = _linearRegressionSlope.Result[index];
		var isTrending = slope > SlopeMaximum || slope < SlopeMinimum;

		Slope[index] = slope;
		_isTrending[index] = isTrending;
	}

	public override void OnRender(IDrawingContext context)
	{
		if (SlopeShadingColor.A > 0)
		{
			if (SlopeRule is SlopeRuleType.Minimum)
			{
				ShadeBetween(context, SlopeMinimum, SlopeMaximum, SlopeShadingColor);
			}
			else
			{
				ShadeBetween(context, SlopeMaximum, int.MaxValue, SlopeShadingColor);
				ShadeBetween(context, SlopeMinimum, int.MinValue, SlopeShadingColor);
			}
		}

		if (SlopeZeroLevelStroke is { IsVisible: true, Color.A: > 0 })
		{
			DrawLevel(context, 0, SlopeZeroLevelStroke);
		}

		if (SlopeThresholdLevelStroke is { IsVisible: true, Color.A: > 0 })
		{
			DrawLevel(context, SlopeMaximum, SlopeThresholdLevelStroke);
			DrawLevel(context, SlopeMinimum, SlopeThresholdLevelStroke);
		}
	}

	private void DrawLevel(IDrawingContext context, double value, Stroke stoke)
	{
		var pointA = new Point
		{
			X = 0,
			Y = ChartScale.GetYCoordinateByValue(value)
		};

		var pointB = new Point
		{
			X = Chart.Width,
			Y = pointA.Y
		};

		context.DrawLine(pointA, pointB, stoke);
	}

	private void ShadeBetween(IDrawingContext context, double firstValue, double secondValue, Color color)
	{
		var pointA = new Point
		{
			X = 0,
			Y = ChartScale.GetYCoordinateByValue(firstValue)
		};

		var pointB = new Point
		{
			X = Chart.Width,
			Y = ChartScale.GetYCoordinateByValue(secondValue)
		};

		context.DrawRectangle(pointA, pointB, color);
	}

	public enum SlopeRuleType
	{
		Maximum,
		Minimum
	}
}

public interface ITrendSlopeFilter
{
	public const string VisualsGroupName = "Visuals";

	[Parameter("Moving Average Type")]
	public MovingAverageType SlopeMaType { get; set; }

	[Parameter("Moving Average Period"), NumericRange(1)]
	public int SlopeMaPeriod { get; set; }

	[Parameter("Linear Regression Slope Period"), NumericRange(1)]
	public int SlopePeriod { get; set; }

	[Parameter("Slope Rule")]
	public TrendSlopeFilter.SlopeRuleType SlopeRule { get; set; }

	[Parameter("Maximum"), NumericRange]
	public double SlopeMaximum { get; set; }

	[Parameter("Minimum"), NumericRange(int.MinValue, 0)]
	public double SlopeMinimum { get; set; }

	[Parameter("Slope Line", GroupName = VisualsGroupName)]
	public Stroke SlopeStroke { get; set; }

	[Parameter("Zero Level", GroupName = VisualsGroupName)]
	public Stroke SlopeZeroLevelStroke { get; set; }

	[Parameter("Threshold Level", GroupName = VisualsGroupName)]
	public Stroke SlopeThresholdLevelStroke { get; set; }

	[Parameter("Invalid Slope Shading", GroupName = VisualsGroupName)]
	public Color SlopeShadingColor { get; set; }

	public void SetDefaults()
	{
		SlopeMaType = MovingAverageType.Exponential;
		SlopeMaPeriod = 14;
		SlopePeriod = 7;
		SlopeRule = TrendSlopeFilter.SlopeRuleType.Minimum;
		SlopeMaximum = 0.75;
		SlopeMinimum = -0.75;
		SlopeStroke = new Stroke { Color = Color.Yellow };
		SlopeZeroLevelStroke = new Stroke { Color = Color.Gray, LineStyle = LineStyle.Dash };
		SlopeThresholdLevelStroke = new Stroke { Color = Color.Gray };
		SlopeShadingColor = Color.New(Color.Red, 0.35f);
	}

	public void AdjustParameters(Parameters parameters)
	{
		if (SlopeMaType != MovingAverageType.StepMA)
			return;

		parameters[nameof(SlopeMaPeriod)].Attributes.Name = "Step Size";
		parameters[nameof(SlopeMaPeriod)].NumericRange!.MinValue = 1;
		parameters[nameof(SlopeMaPeriod)].NumericRange!.MaxValue = int.MaxValue;
	}
}