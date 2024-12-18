using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class DoubleSmoothedStochasticStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("Period 1"), NumericRange(1, int.MaxValue)]
	public int Period1 { get; set; } = 10;

	[Parameter("Period 2"), NumericRange(1, int.MaxValue)]
	public int Period2 { get; set; } = 3;

	[Parameter("Period 3"), NumericRange(1, int.MaxValue)]
	public int Period3 { get; set; } = 3;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 90;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = 10;

	private DoubleSmoothStochastics _dss;

	public DoubleSmoothedStochasticStrategy()
	{
		Name = "DSS Strategy";
		ShortName = "DSS";
		Description = "Double Smoothed Stochastic [DDS] - OB/OS Strategy";
	}

	protected override void Initialize()
	{
		_dss = new DoubleSmoothStochastics(Period1, Period2, Period3) { ShowOnChart = true };
		_dss.OverboughtLevel.Value = OverboughtLevel;
		_dss.OversoldLevel.Value = OversoldLevel;
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		if (_dss[index] >= OverboughtLevel && _dss[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (_dss[index] <= OversoldLevel && _dss[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
