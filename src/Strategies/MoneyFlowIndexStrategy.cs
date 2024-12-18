using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class MoneyFlowIndexStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("Period"), NumericRange(2, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 80;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = 20;

	private MoneyFlowIndex _mfi;

	public MoneyFlowIndexStrategy()
	{
		Name = "MFI Strategy";
		ShortName = "MFI";
		Description = "Money Flow Index [MFI] - OB/OS Strategy";
	}

	protected override void Initialize()
	{
		_mfi = new MoneyFlowIndex(Period) { ShowOnChart = true };
		_mfi.OverboughtLevel.Value = OverboughtLevel;
		_mfi.OversoldLevel.Value = OversoldLevel;
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (_mfi[index] >= OverboughtLevel && _mfi[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (_mfi[index] <= OversoldLevel && _mfi[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
