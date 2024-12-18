using System;
using System.ComponentModel;
using System.Linq.Expressions;

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

	private int _lastRunIndex;
	private IExchangeSession? _currentSession;
	private Series<double> _cumulativeVolume;
	private Series<double> _cumulativeTypicalVolume;
	private Series<double> _cumulativeVariance;

	public VolumeWeightedAveragePrice()
	{
		Name = "Volume Weighted Average Price";
		ShortName = "VWAP";
		IsOverlay = true;
		AutoRescale = false;
	}

	protected override void Initialize()
	{
		_cumulativeVolume = new DataSeries();
		_cumulativeTypicalVolume = new DataSeries();
		_cumulativeVariance = new DataSeries();
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		if (bar == null)
		{
			return;
		}

		var typicalPrice = Bars.TypicalPrice[index];

		double curVWAP;
		var currentSession = Symbol.ExchangeCalendar.GetSession(bar.Time);
		if (currentSession?.StartExchangeDateTime != _currentSession?.StartExchangeDateTime && _lastRunIndex != index)
		{
			_currentSession = currentSession;
			_cumulativeVolume[index] = bar.Volume;
			_cumulativeTypicalVolume[index] = typicalPrice * bar.Volume;
			curVWAP = typicalPrice;
			_cumulativeVariance[index] = 0;
		}
		else
		{
			_cumulativeVolume[index] = _cumulativeVolume[index - 1] + bar.Volume;
			_cumulativeTypicalVolume[index] = _cumulativeTypicalVolume[index - 1] + bar.Volume * typicalPrice;
			curVWAP = _cumulativeTypicalVolume[index] / _cumulativeVolume[index];
			_cumulativeVariance[index] = _cumulativeVariance[index - 1] + Math.Pow(typicalPrice - curVWAP, 2) * bar.Volume;
		}

		_lastRunIndex = index;
		Result[index] = curVWAP;

		var deviation = Math.Sqrt(_cumulativeVariance[index] / _cumulativeVolume[index]);
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

			upperBand[index] = curVWAP + deviation * multiplier;
			lowerBand[index] = curVWAP - deviation * multiplier;
		}
	}

	private static int ToInteger(DateTime t)
	{
		var hr = t.Hour * 10000;
		var min = t.Minute * 100;
		return hr + min + t.Second;
	}
}