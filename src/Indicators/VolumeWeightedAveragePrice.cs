using Tickblaze.Scripts.Misc;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Volume Weighted Average Price [VWAP]
/// </summary>
public partial class VolumeWeightedAveragePrice : Indicator
{
	[Parameter("Reset Period")]
	public VwapResetPeriod ResetPeriod { get; set; } = VwapResetPeriod.Day;

	[Parameter("Show Band 1")]
	public bool ShowBand1 { get; set; }

	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Multiplier { get; set; } = 1;

	[NumericRange(0, 100)]
	[Parameter("Band 1 Fill Shading Opacity %", Description = "Opacity of the shading between VWAP and band 1")]
	public int Band1FillShadingOpacity { get; set; } = 20;

	[Parameter("Show Band 2")]
	public bool ShowBand2 { get; set; }

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Multiplier { get; set; } = 2;

	[NumericRange(0, 100)]
	[Parameter("Band 2 Fill Shading Opacity %", Description = "Opacity of the shading between band 1 and 2")]
	public int Band2FillShadingOpacity { get; set; } = 15;

	[Parameter("Show Band 3")]
	public bool ShowBand3 { get; set; }

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Multiplier { get; set; } = 3;

	[NumericRange(0, 100)]
	[Parameter("Band 3 Fill Shading Opacity %", Description = "Opacity of the shading between band 2 and 3")]
	public int Band3FillShadingOpacity { get; set; } = 10;

	[Plot("Vwap")]
	public PlotSeries Result { get; set; } = new(Color.Cyan);

	[Plot("Band1 Upper")]
	public PlotSeries Band1Upper { get; set; } = new(Color.Green);

	[Plot("Band1 Lower")]
	public PlotSeries Band1Lower { get; set; } = new(Color.Green);

	[Plot("Band2 Upper")]
	public PlotSeries Band2Upper { get; set; } = new(Color.Yellow);

	[Plot("Band2 Lower")]
	public PlotSeries Band2Lower { get; set; } = new(Color.Yellow);

	[Plot("Band3 Upper")]
	public PlotSeries Band3Upper { get; set; } = new(Color.Red);

	[Plot("Band3 Lower")]
	public PlotSeries Band3Lower { get; set; } = new(Color.Red);

	private VwapCalculator _vwapCalculator;

	public VolumeWeightedAveragePrice()
	{
		Name = "Volume Weighted Average Price";
		ShortName = "VWAP";
		IsOverlay = true;
		AutoRescale = false;
	}

	protected override void Initialize()
	{
		_vwapCalculator = new VwapCalculator(Bars, Symbol, ResetPeriod);

		var upperLast = Result;
		var lowerLast = Result;

		for (var i = 1; i <= 3; i++)
		{
			var (show, opacity, upperBand, lowerBand) = i switch
			{
				1 => (ShowBand1, Band1FillShadingOpacity, Band1Upper, Band1Lower),
				2 => (ShowBand2, Band2FillShadingOpacity, Band2Upper, Band2Lower),
				3 => (ShowBand3, Band3FillShadingOpacity, Band3Upper, Band3Lower),
				_ => throw new NotImplementedException(),
			};

			if (!show)
			{
				continue;
			}

			ShadeBetween(upperBand, upperLast, upperBand.Color, upperBand.Color, opacity / 100f);
			ShadeBetween(lowerBand, lowerLast, lowerBand.Color, lowerBand.Color, opacity / 100f);

			upperLast = upperBand;
			lowerLast = lowerBand;
		}
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		if (bar == null)
		{
			return;
		}

		_vwapCalculator.Update(index, out var isReset);

		if (isReset)
		{
			foreach (var plot in Plots)
			{
				plot.IsLineBreak[index] = true;
			}
		}

		Result[index] = _vwapCalculator.VWAP;

		CalculateBand(index, ShowBand1, Band1Lower, Band1Upper, Band1Multiplier);
		CalculateBand(index, ShowBand2, Band2Lower, Band2Upper, Band2Multiplier);
		CalculateBand(index, ShowBand3, Band3Lower, Band3Upper, Band3Multiplier);
	}

	private void CalculateBand(int barIndex, bool showBand, PlotSeries lowerBand, PlotSeries upperBand, double bandMultiplier)
	{
		if (!showBand)
		{
			return;
		}

		upperBand[barIndex] = _vwapCalculator.VWAP + bandMultiplier * _vwapCalculator.Deviation;
		lowerBand[barIndex] = _vwapCalculator.VWAP - bandMultiplier * _vwapCalculator.Deviation;
	}
}