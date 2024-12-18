using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class StochasticRelativeStrengthIndexStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int StochRsiPeriod { get; set; } = 14;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 0.8;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = 0.2;

	private StochasticRelativeStrengthIndex _stochRsi;

	public StochasticRelativeStrengthIndexStrategy()
	{
		Name = "Stoch RSI Strategy";
		ShortName = "Stoch RSI";
		Description = "Stochastic Relative Strength Index [Stoch RSI] - OB/OS Strategy";
	}

	protected override void Initialize()
	{
		_stochRsi = new StochasticRelativeStrengthIndex(Bars.Close, StochRsiPeriod) { ShowOnChart = true };
		_stochRsi.OverboughtLevel.Value = OverboughtLevel;
		_stochRsi.OversoldLevel.Value = OversoldLevel;
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (_stochRsi[index] >= OverboughtLevel && _stochRsi[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (_stochRsi[index] <= OversoldLevel && _stochRsi[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
