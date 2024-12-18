using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class WilliamsPercentRStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int WprPeriod { get; set; } = 14;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = -20;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = -80;

	private WilliamsPercentR _wpr;

	public WilliamsPercentRStrategy()
	{
		Name = "Williams %R Strategy";
		ShortName = "%R";
		Description = "Williams %R - OB/OS Strategy";
	}

	protected override void Initialize()
	{
		_wpr = new WilliamsPercentR(WprPeriod) { ShowOnChart = true };
		_wpr.OverboughtLevel.Value = OverboughtLevel;
		_wpr.OversoldLevel.Value = OversoldLevel;
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (_wpr[index] >= OverboughtLevel && _wpr[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (_wpr[index] <= OversoldLevel && _wpr[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
