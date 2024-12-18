using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class StochasticStrategy : BaseStopsAndTargetsStrategy
{
	[Parameter("%K Periods"), NumericRange(1, int.MaxValue)]
	public int KPeriods { get; set; } = 9;

	[Parameter("%K Slowing"), NumericRange(1, int.MaxValue)]
	public int KSlowing { get; set; } = 3;

	[Parameter("%D Periods"), NumericRange(1, int.MaxValue)]
	public int DPeriods { get; set; } = 9;

	[Parameter("MA Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Parameter("Output")]
	public StochOutputType Output { get; set; } = StochOutputType.PercentK;

	[Parameter("Overbought Level")]
	public double OverboughtLevel { get; set; } = 80;

	[Parameter("Oversold Level")]
	public double OversoldLevel { get; set; } = 20;

	public enum StochOutputType
	{
		[DisplayName("%K")]
		PercentK,

		[DisplayName("%D")]
		PercentD,
	}

	private StochasticOscillator _stoch;

	public StochasticStrategy()
	{
		Name = "Stochastic Strategy";
		ShortName = "Stoch";
		Description = "Stochastic Oscillator [Stoch] - OB/OS Strategy";
	}

	protected override void Initialize()
	{
		_stoch = new StochasticOscillator(KPeriods, KSlowing, DPeriods, SmoothingType) { ShowOnChart = true };
		_stoch.OverboughtLevel.Value = OverboughtLevel;
		_stoch.OversoldLevel.Value = OversoldLevel;
	}

	protected override void OnBar(int index)
	{
		if (index == 0)
		{
			return;
		}

		var stoch = Output is StochOutputType.PercentK ? _stoch.PercentK : _stoch.PercentD;
		if (stoch[index] >= OverboughtLevel && stoch[index - 1] < OverboughtLevel)
		{
			TryEnterMarket(OrderDirection.Short);
		}
		else if (stoch[index] <= OversoldLevel && stoch[index - 1] > OversoldLevel)
		{
			TryEnterMarket(OrderDirection.Long);
		}
	}
}
