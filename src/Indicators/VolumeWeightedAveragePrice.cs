using Tickblaze.Scripts.Misc;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Volume Weighted Average Price [VWAP]
/// </summary>
public partial class VolumeWeightedAveragePrice : Indicator
{
	[Parameter("Show Band 1")]
	public bool ShowBand1 { get; set; } = false;
	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Multiplier { get; set; } = 0.75;

	[Parameter("Show Band 2")]
	public bool ShowBand2 { get; set; } = false;
	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Multiplier { get; set; } = 1.75;

	[Parameter("Show Band 3")]
	public bool ShowBand3 { get; set; } = false;

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Multiplier { get; set; } = 2.75;

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

	private VwapCalculator? _vwapTracker;

	public VolumeWeightedAveragePrice()
	{
		Name = "Volume Weighted Average Price";
		ShortName = "VWAP";
		IsOverlay = true;
		AutoRescale = false;
	}

	protected override void Initialize()
	{
		_vwapTracker ??= new VwapCalculator(Bars, Symbol);
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		if (bar == null)
		{
			return;
		}

		_vwapTracker!.Update(index);
		Result[index] = _vwapTracker.VWAP;

		for (var i = 0; i < 3; i++)
		{
			var showBand = i switch
			{
				0 => ShowBand1,
				1 => ShowBand2,
				2 => ShowBand3
			};

			if (!showBand)
			{
				continue;
			}

			var upperBand = i switch
			{
				0 => Band1Upper,
				1 => Band2Upper,
				2 => Band3Upper
			};
			
			var lowerBand = i switch
			{
				0 => Band1Lower,
				1 => Band2Lower,
				2 => Band3Lower
			};

			var multiplier = i switch
			{
				0 => Band1Multiplier,
				1 => Band2Multiplier,
				2 => Band3Multiplier
			};

			upperBand[index] = _vwapTracker.VWAP + _vwapTracker.Deviation * multiplier;
			lowerBand[index] = _vwapTracker.VWAP - _vwapTracker.Deviation * multiplier;
		}
	}
}