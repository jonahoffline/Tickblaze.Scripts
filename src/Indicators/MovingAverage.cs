﻿namespace Tickblaze.Scripts.Indicators;

public enum MovingAverageType
{
	Simple,
	
	Exponential,

	[DisplayName("Double Exponential")]
	DoubleExponential,

	[DisplayName("Triple Exponential")]
	TripleExponential,

	[DisplayName("Zero Lag Exponential")]
	ZeroLagExponential,

	Triangular,

	[DisplayName("Sine Weighted")]
	SineWeighted,

	Smoothed,
	
	Weighted,
	
	[DisplayName("Welles Wilder")]
	WellesWilder,

	Hull,

	[DisplayName("Volume Weighted")]
	VolumeWeighted,

	[DisplayName("Step MA")]
	StepMA
}

/// <summary>
/// Moving Average [MA]
/// </summary>
public partial class MovingAverage : Indicator
{
	/// <summary>
	/// The source series of data on which the moving average is calculated.
	/// </summary>
	/// <value>
	/// An instance of <see cref="ISeries{double}"/> representing the source data series.
	/// </value>
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	/// <summary>
	/// The period over which the moving average is calculated.
	/// </summary>
	/// <value>
	/// An integer value representing the period (number of data points) for the moving average.
	/// The default value is 14.
	/// </value>
	/// <remarks>
	/// The value must be between 1 and 999, inclusive, with a step of 1.
	/// </remarks>
	[Parameter("Period"), NumericRange(1, 999)]
	public int Period { get; set; } = 14;

	/// <summary>
	/// The type of moving average to be used.
	/// </summary>
	/// <value>
	/// A <see cref="MovingAverageType"/> enum value specifying the type of moving average.
	/// The default value is <see cref="MovingAverageType.Simple"/>.
	/// </value>
	[Parameter("Type")]
	public MovingAverageType Type { get; set; } = MovingAverageType.Simple;

	/// <summary>
	/// The plot series used to visualize the moving average results.
	/// </summary>
	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#2962ff", LineStyle.Solid, 1);

	public override string DisplayName => Type switch
	{
		MovingAverageType.Simple => "SMA",
		MovingAverageType.Exponential => "EMA",
		MovingAverageType.DoubleExponential => "DEMA",
		MovingAverageType.TripleExponential => "TEMA",
		MovingAverageType.ZeroLagExponential => "ZLEMA",
		MovingAverageType.Triangular => "TMA",
		MovingAverageType.SineWeighted => "SWMA",
		MovingAverageType.Smoothed => "SMMA",
		MovingAverageType.Weighted => "WMA",
		MovingAverageType.WellesWilder => "WWMA",
		MovingAverageType.Hull => "HMA",
		MovingAverageType.VolumeWeighted => "VWMA",
		_ => throw new NotImplementedException()
	};

	private ISeries<double> _result;

	public MovingAverage()
	{
		Name = "Moving Average";
		ShortName = "MA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_result = Type switch
		{
			MovingAverageType.Simple => new SimpleMovingAverage(Source, Period).Result,
			MovingAverageType.Exponential => new ExponentialMovingAverage(Source, Period).Result,
			MovingAverageType.DoubleExponential => new DoubleExponentialMovingAverage(Source, Period).Result,
			MovingAverageType.TripleExponential => new TripleExponentialMovingAverage(Source, Period).Result,
			MovingAverageType.ZeroLagExponential => new ZeroLagExponentialMovingAverage(Source, Period).Result,
			MovingAverageType.Triangular => new TriangularMovingAverage(Source, Period).Result,
			MovingAverageType.SineWeighted => new SineWeightedMovingAverage(Source, Period).Result,
			MovingAverageType.Smoothed => new SmoothedMovingAverage(Source, Period).Result,
			MovingAverageType.Weighted => new WeightedMovingAverage(Source, Period).Result,
			MovingAverageType.WellesWilder => new ExponentialMovingAverage(Source, 2 * Period - 1).Result,
			MovingAverageType.Hull => new HullMovingAverage(Source, Period).Result,
			MovingAverageType.VolumeWeighted => new VolumeWeightedMovingAverage(Source, Period).Result,
			MovingAverageType.StepMA => new TrendStepper(Source, Period, 0, Color.Transparent, Color.Transparent).Result,
			_ => throw new NotImplementedException()
		};

		if (Type == MovingAverageType.StepMA)
			Result.PlotStyle = PlotStyle.Stair;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (Type != MovingAverageType.StepMA)
			return parameters;
		
		parameters[nameof(Period)].Attributes.Name = "Step Size";
		parameters[nameof(Period)].NumericRange!.MinValue = 1;
		parameters[nameof(Period)].NumericRange!.MaxValue = int.MaxValue;
		return parameters;
	}

	protected override void Calculate(int index)
	{
		Result[index] = _result[index];
	}
}