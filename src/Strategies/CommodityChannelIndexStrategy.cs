using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class CommodityChannelIndexStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 20;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 100;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = -100;

	private CommodityChannelIndex _cci;

	public CommodityChannelIndexStrategy()
	{
		Name = "CCI Strategy";
		ShortName = "CCI";
		Description = "Commodity Channel Index [CCI] - OB/OS Strategy";
	}

	protected override void Initialize()
	{
		_cci = new CommodityChannelIndex(Period) { ShowOnChart = true };
		_cci.OverboughtLevel.Value = OverboughtLevel;
		_cci.OversoldLevel.Value = OversoldLevel;
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (_cci[index] >= OverboughtLevel && _cci[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (_cci[index] <= OversoldLevel && _cci[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
