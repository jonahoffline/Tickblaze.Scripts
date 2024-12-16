using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.Strategies;

public abstract class BaseStopsAndTargetsStrategy : Strategy
{
	[Parameter("Stop Loss Type")]
	public StopTargetDistanceType StopLossType { get; set; } = StopTargetDistanceType.Dollars;

	[Parameter("Stop Loss"), NumericRange(0)]
	public double StopLoss { get; set; } = 0;

	[Parameter("Take Profit Type")]
	public StopTargetDistanceType TakeProfitType { get; set; } = StopTargetDistanceType.Dollars;

	[Parameter("Take Profit"), NumericRange(0)]
	public double TakeProfit { get; set; } = 0;

	protected void PlaceStopLossAndTarget(IOrder order, double entryPrice, OrderDirection orderDirection)
	{
		if (StopLoss > 0)
		{
			switch (StopLossType)
			{
			case StopTargetDistanceType.Dollars:
				SetStopLossTicks(order, (int)Math.Max(StopLoss / (Symbol.TickValue * order.Quantity), 1), "SL");
				break;
			case StopTargetDistanceType.DollarsPerQuantity:
				SetStopLossTicks(order, (int)Math.Max(StopLoss / Symbol.TickValue, 1), "SL");
				break;
			case StopTargetDistanceType.Ticks:
				SetStopLossTicks(order, (int)Math.Max(StopLoss, 1), "SL");
				break;
			case StopTargetDistanceType.Points:
				SetStopLossTicks(order, (int)Math.Max(StopLoss * Symbol.TicksPerPoint, 1), "SL");
				break;
			case StopTargetDistanceType.PercentOfPrice:
				SetStopLossPercent(order, Math.Max(StopLoss, 0.0001), "SL");
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		if (TakeProfit > 0)
		{
			switch (TakeProfitType)
			{
			case StopTargetDistanceType.Dollars:
				SetTakeProfitTicks(order, (int)Math.Max(TakeProfit / (Symbol.TickValue * order.Quantity), 1), "TP");
				break;
			case StopTargetDistanceType.DollarsPerQuantity:
				SetTakeProfitTicks(order, (int)Math.Max(TakeProfit / Symbol.TickValue, 1), "TP");
				break;
			case StopTargetDistanceType.Ticks:
				SetTakeProfitTicks(order, (int)Math.Max(TakeProfit, 1), "TP");
				break;
			case StopTargetDistanceType.Points:
				SetTakeProfitTicks(order, (int)Math.Max(TakeProfit * Symbol.TicksPerPoint, 1), "TP");
				break;
			case StopTargetDistanceType.PercentOfPrice:
				SetTakeProfitPercent(order, Math.Max(TakeProfit, 0.0001), "TP");
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}

public enum StopTargetDistanceType
{
	[DisplayName("$")]
	Dollars,
	[DisplayName("$/Unit")]
	DollarsPerQuantity,
	Ticks,
	Points,
	[DisplayName("% of Price")]
	PercentOfPrice
}