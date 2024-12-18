using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class RelativeStrengthIndexStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Signal Type")]
	public MovingAverageType SignalType { get; set; } = MovingAverageType.Simple;

	[Parameter("Signal Period"), NumericRange(1, int.MaxValue)]
	public int SignalPeriod { get; set; } = 14;

	[Parameter("Output")]
	public RsiOutputType Output { get; set; } = RsiOutputType.Result;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 70;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = 30;

	public enum RsiOutputType
	{
		[DisplayName("Result")]
		Result,

		[DisplayName("Average")]
		Average,
	}

	private RelativeStrengthIndex _rsi;

	public RelativeStrengthIndexStrategy()
	{
		Name = "RSI Strategy";
		ShortName = "RSI";
		Description = "Relative Strength Index [RSI] - OB/OS Strategy";
	}

	protected override void Initialize()
	{
		_rsi = new RelativeStrengthIndex(Bars.Close, Period, SignalType, SignalPeriod) { ShowOnChart = true };
		_rsi.OverboughtLevel.Value = OverboughtLevel;
		_rsi.OversoldLevel.Value = OversoldLevel;
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		var rsi = Output is RsiOutputType.Result ? _rsi.Result : _rsi.Average;
		if (rsi[index] >= OverboughtLevel && rsi[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (rsi[index] <= OversoldLevel && rsi[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
