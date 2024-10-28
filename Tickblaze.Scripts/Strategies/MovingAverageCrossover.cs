using System.ComponentModel;
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

//[Browsable(false)]
public class MovingAverageCrossover : Strategy
{
	[Parameter("MA Type")]
	public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Simple;

	[Parameter("Fast Period")]
	public int FastPeriod { get; set; } = 12;

	[Parameter("Slow Period")]
	public int SlowPeriod { get; set; } = 26;

	private MovingAverage _fastMovingAverage, _slowMovingAverage;
	private Series<bool> _isBullishTrend;

	public MovingAverageCrossover()
	{
		Name = "Moving Average Crossover";
		Description = "The Moving Average Crossover Strategy detects trends by tracking crossovers between fast and slow moving averages. A bullish crossover triggers a buy order, while a bearish crossover triggers a sell order, aiming to capture early trend changes.";
	}

	protected override void Initialize()
	{
		_fastMovingAverage = new MovingAverage(Bars.Close, FastPeriod, MovingAverageType)
		{
			ShowOnChart = true
		};

		_slowMovingAverage = new MovingAverage(Bars.Close, SlowPeriod, MovingAverageType)
		{
			ShowOnChart = true
		};

		_isBullishTrend = new();
	}

	protected override void OnBar(int index)
	{
		var fastMovingAverage = _fastMovingAverage[index];
		var slowMovingAverage = _slowMovingAverage[index];

		if (fastMovingAverage > slowMovingAverage)
		{
			_isBullishTrend[index] = true;
		}
		else if (fastMovingAverage < slowMovingAverage)
		{
			_isBullishTrend[index] = false;
		}
		else if (index > 0)
		{
			_isBullishTrend[index] = _isBullishTrend[index - 1];
		}

		if (index == 0)
		{
			return;
		}

		var isBullishCrossover = _isBullishTrend[index] && !_isBullishTrend[index - 1];
		if (isBullishCrossover)
		{
			ClosePosition();
			ExecuteMarketOrder(OrderAction.Buy, 1);
		}

		var isBearishCrossover = !_isBullishTrend[index] && _isBullishTrend[index - 1];
		if (isBearishCrossover)
		{
			ClosePosition();
			ExecuteMarketOrder(OrderAction.Sell, 1);
		}
	}
}
