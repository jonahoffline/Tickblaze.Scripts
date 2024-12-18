using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

[System.ComponentModel.Browsable(false)]
public class OverboughtOversoldStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("Oscillator")]
	public Oscillator OscillatorType { get; set; } = Oscillator.CommodityChannelIndex;

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int CciPeriod { get; set; } = 20;

	[Parameter("Period"), NumericRange(2, int.MaxValue)]
	public int MfiPeriod { get; set; } = 14;

	[Parameter("Output")]
	public RsiOutputType RsiOutput { get; set; } = RsiOutputType.Result;

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int RsiPeriod { get; set; } = 14;

	[Parameter("Signal Type")]
	public MovingAverageType RsiSignalType { get; set; } = MovingAverageType.Simple;

	[Parameter("Signal Period"), NumericRange(1, int.MaxValue)]
	public int RsiSignalPeriod { get; set; } = 14;

	[Parameter("Period 1"), NumericRange(1, int.MaxValue)]
	public int DssPeriod1 { get; set; } = 10;

	[Parameter("Period 2"), NumericRange(1, int.MaxValue)]
	public int DssPeriod2 { get; set; } = 3;

	[Parameter("Period 3"), NumericRange(1, int.MaxValue)]
	public int DssPeriod3 { get; set; } = 3;

	[Parameter("Output")]
	public StochOutputType StochOutput { get; set; } = StochOutputType.PercentK;

	[Parameter("%K Periods"), NumericRange(1, int.MaxValue)]
	public int StochKPeriods { get; set; } = 9;

	[Parameter("%K Slowing"), NumericRange(1, int.MaxValue)]
	public int StochKSlowing { get; set; } = 3;

	[Parameter("%D Periods"), NumericRange(1, int.MaxValue)]
	public int StochDPeriods { get; set; } = 9;

	[Parameter("MA Type")]
	public MovingAverageType StochSmoothingType { get; set; } = MovingAverageType.Simple;

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int StochRsiPeriod { get; set; } = 14;

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int WprPeriod { get; set; } = 14;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 100;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = -100;

	public enum Oscillator
	{
		[DisplayName("Commodity Channel Index [CCI]")]
		CommodityChannelIndex,

		[DisplayName("Money Flow Index [MFI]")]
		MoneyFlowIndex,

		[DisplayName("Relative Strength Index [RSI]")]
		RelativeStrengthIndex,

		[DisplayName("Double Smoothed Stochastic [DSS]")]
		DoubleSmoothStochastics,

		[DisplayName("Stochastic Oscillator")]
		StochasticOscillator,

		[DisplayName("Stochastic Relative Strength Index [StochRSI]")]
		StochasticRelativeStrengthIndex,

		[DisplayName("Williams %R")]
		WilliamsPercentR,
	}

	public enum RsiOutputType
	{
		[DisplayName("Result")]
		Result,

		[DisplayName("Average")]
		Average,
	}

	public enum StochOutputType
	{
		[DisplayName("%K")]
		PercentK,

		[DisplayName("%D")]
		PercentD,
	}

	private ISeries<double> _oscillator;
	private Dictionary<Oscillator, (string[] Parameters, double OverboughtLevel, double OversoldLevel)> _oscillatorParameters = new()
	{
		{
			Oscillator.CommodityChannelIndex, ([nameof(CciPeriod)], 100, -100)
		},
		{
			Oscillator.MoneyFlowIndex, ([nameof(MfiPeriod)], 80, 20)
		},
		{
			Oscillator.RelativeStrengthIndex, (
			[
				nameof(RsiPeriod),
				nameof(RsiSignalType),
				nameof(RsiSignalPeriod),
				nameof(RsiOutput)
			], 70, 30)
		},
		{
			Oscillator.DoubleSmoothStochastics, (
			[
				nameof(DssPeriod1),
				nameof(DssPeriod2),
				nameof(DssPeriod3)
			], 90, 10)
		},
		{
			Oscillator.StochasticOscillator, (
			[
				nameof(StochKPeriods),
				nameof(StochKSlowing),
				nameof(StochDPeriods),
				nameof(StochSmoothingType),
				nameof(StochOutput)
			], 80, 20)
		},
		{
			Oscillator.StochasticRelativeStrengthIndex, ([nameof(StochRsiPeriod)], 0.8, 0.2)
		},
		{
			Oscillator.WilliamsPercentR, ([nameof(WprPeriod)], -20, -80)
		}
	};

	public OverboughtOversoldStrategy()
	{
		Name = "Overbought/Oversold Strategy";
		ShortName = "OBOS";
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		foreach (var (oscillatorType, oscillator) in _oscillatorParameters)
		{
			if (OscillatorType == oscillatorType)
			{
				OverboughtLevel = oscillator.OverboughtLevel;
				OversoldLevel = oscillator.OversoldLevel;
			}
			else
			{
				foreach (var parameter in oscillator.Parameters)
				{
					parameters.Remove(parameter);
				}
			}
		}

		return parameters;
	}

	protected override void Initialize()
	{
		_oscillator = GetOscillatorOutputSeries();
	}

	private PlotSeries GetOscillatorOutputSeries()
	{
		if (OscillatorType is Oscillator.CommodityChannelIndex)
		{
			var cci = new CommodityChannelIndex(CciPeriod);
			cci.OverboughtLevel.Value = OverboughtLevel;
			cci.OversoldLevel.Value = OversoldLevel;
			cci.ShowOnChart = true;

			return cci.Result;
		}
		else if (OscillatorType is Oscillator.MoneyFlowIndex)
		{
			var mfi = new MoneyFlowIndex(MfiPeriod);
			mfi.OverboughtLevel.Value = OverboughtLevel;
			mfi.OversoldLevel.Value = OversoldLevel;
			mfi.ShowOnChart = true;

			return mfi.Result;
		}
		else if (OscillatorType is Oscillator.RelativeStrengthIndex)
		{
			var rsi = new RelativeStrengthIndex(Bars.Close, RsiPeriod, RsiSignalType, RsiSignalPeriod);
			rsi.OverboughtLevel.Value = OverboughtLevel;
			rsi.OversoldLevel.Value = OversoldLevel;
			rsi.ShowOnChart = true;

			return RsiOutput is RsiOutputType.Result ? rsi.Result : rsi.Average;
		}
		else if (OscillatorType is Oscillator.DoubleSmoothStochastics)
		{
			var dss = new DoubleSmoothStochastics(DssPeriod1, DssPeriod2, DssPeriod3);
			dss.OverboughtLevel.Value = OverboughtLevel;
			dss.OversoldLevel.Value = OversoldLevel;
			dss.ShowOnChart = true;

			return dss.Result;
		}
		else if (OscillatorType is Oscillator.StochasticOscillator)
		{
			var stoch = new StochasticOscillator(StochKPeriods, StochKSlowing, StochDPeriods, StochSmoothingType);
			stoch.OverboughtLevel.Value = OverboughtLevel;
			stoch.OversoldLevel.Value = OversoldLevel;
			stoch.ShowOnChart = true;

			return StochOutput is StochOutputType.PercentK ? stoch.PercentK : stoch.PercentD;
		}
		else if (OscillatorType is Oscillator.StochasticRelativeStrengthIndex)
		{
			var stochRsi = new StochasticRelativeStrengthIndex(Bars.Close, StochRsiPeriod);
			stochRsi.OverboughtLevel.Value = OverboughtLevel;
			stochRsi.OversoldLevel.Value = OversoldLevel;
			stochRsi.ShowOnChart = true;

			return stochRsi.Result;
		}
		else if (OscillatorType is Oscillator.WilliamsPercentR)
		{
			var wpr = new WilliamsPercentR(WprPeriod);
			wpr.OverboughtLevel.Value = OverboughtLevel;
			wpr.OversoldLevel.Value = OversoldLevel;
			wpr.ShowOnChart = true;

			return wpr.Result;
		}

		throw new NotImplementedException();
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (_oscillator[index] >= OverboughtLevel && _oscillator[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (_oscillator[index] <= OversoldLevel && _oscillator[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
