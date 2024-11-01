using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.TradeManagementStrategies;

public class TestJon : TradeManagementStrategy
{
	[NumericRange(0, int.MaxValue)]
	[Parameter("Stop-loss distance (ticks)", Description = "Enter the distance from your entry in ticks, that you would like to set your stop loss. Entering zero (0) means you have no stop loss.")]
	public int StopLossTicks { get; set; } = 16;

	[Parameter("Position size type")]
	public SizeType PositionSizeType { get; set; }

	[NumericRange(0, double.MaxValue)]
	[Parameter("Position size value")]
	public double PositionSize { get; set; } = 500;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #1 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #1. Entering zero (0) means you have no Target #1.")]
	public int FirstTakeProfitTicks { get; set; } = 8;

	[NumericRange(0, double.MaxValue)]
	[Parameter("Take-profit #1 size (%)", Description = "Enter how much % of your position you would like to close at Target #1.")]
	public double FirstTakeProfitSizePercent { get; set; } = 70;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #2 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #2. Entering zero (0) means you have no Target #2.")]
	public int SecondTakeProfitTicks { get; set; } = 24;

	[NumericRange(0, double.MaxValue)]
	[Parameter("Take-profit #2 size (%)", Description = "Enter how much % of your position you would like to close at Target #2.")]
	public double SecondTakeProfitSizePercent { get; set; } = 30;

	[Parameter("Breakeven stop-loss enabled?", Description = "To make the stop loss move automatically once a certain amount of profit has been gained, set this to True.")]
	public bool EnableAutoBreakeven { get; set; } = false;

	[Parameter("Breakeven trigger (ticks)", Description = "Enter the number of ticks of profit to trigger the stop loss to move.")]
	public int BreakevenTriggerTicks { get; set; } = 8;

	[Parameter("Breakeven offset (ticks)", Description = "Enter the number of ticks of profit you want to protect, once the Breakeven Trigger In Ticks has been reached. A positive number will protect that many ticks, a negative number will leave that many ticks at risk, and allow the trade more room to move before closing.")]
	public int BreakevenOffsetTicks { get; set; } = 1;

	private int DirectionAsInt { get; set; }

	private bool BreakEvenValid => EnableAutoBreakeven && BreakevenTriggerTicks > 0;

	private bool ShouldStopTradeManagementStrategy
	{
		get
		{
			// Stop immediately if there is no break even.
			if (!BreakEvenValid)
			{
				return true;
			}

			// If we're waiting for a break even, stop if our position is no longer valid and all of our entry orders were submitted and reached a final state
			if (Position?.Direction != (DirectionAsInt == 1 ? OrderDirection.Long : OrderDirection.Short) && _orderData.Count > 0 && _orderData.TrueForAll(group => group.Entry.Status != OrderStatus.Pending))
			{
				return true;
			}

			return false;
		}
	}

	private readonly List<OrderData> _orderData = [];

	public TestJon()
	{
		Name = "OCO - Ticks (Jon)";
		Description = @"This TMS script calculates the size of your position based on your Stop Loss Distance (in ticks) and your risk (Calculated in terms of Dollar amount, Percentage Risk or Static Units Risk). It can place 1 or 2 take-profit orders (specified in ticks). You also have the option to set an automatic break-even stop loss function.";
	}

	private void TryMoveToBreakEven()
	{
		if (ShouldStopTradeManagementStrategy)
		{
			Stop();
			return;
		}

		if (!BreakEvenValid || Position == null)
		{
			return;
		}

		var price = Bars.Close[^1];
		var ticksInProfit = (int)Math.Round((price - Position.EntryPrice) / Symbol.TickSize) * DirectionAsInt;
		if (ticksInProfit < BreakevenTriggerTicks)
		{
			return;
		}

		var stopPrice = Position.EntryPrice + BreakevenOffsetTicks * DirectionAsInt * Symbol.TickSize;
		foreach (var group in _orderData)
		{
			ModifyOrder(group.StopLoss, group.StopLoss.Quantity, stopPrice, null);
		}

		Stop();
	}

	protected override void OnEntryOrder(IOrder order)
	{
		CancelOrder(order);

		DirectionAsInt = order.Direction is OrderDirection.Long ? 1 : -1;
		var action = order.Direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		var quantity = (double)CalculateQuantity(PositionSizeType, PositionSize, StopLossTicks, RoundingMode.Down);
		var remainingQuantity = quantity;

		for (var tpIdx = 0; tpIdx < 2 && remainingQuantity > 0; tpIdx++)
		{
			var tpTicks = tpIdx == 0 ? FirstTakeProfitTicks : SecondTakeProfitTicks;
			var tpQuantityPercent = tpIdx == 0 ? FirstTakeProfitSizePercent : SecondTakeProfitSizePercent;
			if (tpTicks == 0 || tpQuantityPercent == 0)
				continue;

			var orderGroupQuantity = Math.Min(remainingQuantity, quantity * tpQuantityPercent / 100);
			remainingQuantity -= orderGroupQuantity;

			_orderData.Add(new OrderData
			{
				Entry = order.Type switch
				{
					OrderType.Stop => PlaceStopOrder(action, orderGroupQuantity, order.StopPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.Limit => PlaceLimitOrder(action, orderGroupQuantity, order.LimitPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.StopLimit => PlaceStopLimitOrder(action, orderGroupQuantity, order.StopPrice, order.LimitPrice, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					OrderType.Market => ExecuteMarketOrder(action, orderGroupQuantity, order.TimeInForce, $"{order.Direction.ToString()[0]}E"),
					_ => throw new ArgumentOutOfRangeException()
				}
			});

			_orderData[^1].ProfitTarget = SetTakeProfit(_orderData[^1].Entry, order.Price + tpTicks * Symbol.TickSize * DirectionAsInt);
			_orderData[^1].StopLoss = SetStopLoss(_orderData[^1].Entry, order.Price - StopLossTicks * Symbol.TickSize * DirectionAsInt);
		}

		if (ShouldStopTradeManagementStrategy)
		{
			Stop();
		}
	}

	private decimal CalculateQuantity(SizeType sizeType, double size, int distanceTicks, RoundingMode roundingMode)
	{
		if (sizeType is SizeType.Units)
		{
			return Symbol.NormalizeVolume(size, roundingMode);
		}

		var risk = sizeType is SizeType.EquityRisk ? size : Account.BuyingPower * (size / 100);
		return Math.Max(Symbol.MinimumVolume, Symbol.NormalizeVolume(risk / (distanceTicks * Symbol.TickValue), roundingMode));
	}

	protected override void OnOrderUpdate(IOrder order)
	{
		TryMoveToBreakEven();
	}

	protected override void OnPositionUpdate()
	{
		TryMoveToBreakEven();
	}

	protected override void OnBarUpdate()
	{
		TryMoveToBreakEven();
	}

	public enum SizeType
	{
		Units,
		EquityRisk,
		EquityRiskPercent
	}
	
	public class OrderData
	{
		public IOrder Entry;
		public IOrder StopLoss;
		public IOrder ProfitTarget;
	}
}
