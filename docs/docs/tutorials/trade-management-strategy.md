# Writting Trade Management System (TMS) for Tickbklaze

## Overview

The `TradeManagementStrategy` is a base class allowing you to automate order management.

The strategy remains active (i.e., continues to process updates and trigger logic) until the `Stop()` function is explicitly called. This ensures that all overridden methods (`OnEntryOrder`, `OnOrderUpdate`, `OnPositionUpdate`, and `OnBarUpdate`) are continuously called, allowing the strategy to execute its logic dynamically.

---

## Basic Example

The `BasicTradeManagementStrategy` is a trading strategy class designed to manage trades by setting take profit and stop loss levels. It also supports trailing stop functionality to lock in profits as the market moves in favor of the trade.

### Source Code
Below is the complete source code for the `BasicTradeManagementStrategy` class:

```csharp
using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.Tutorials;

public class BasicTradeManagementStrategy : TradeManagementStrategy
{
    [Parameter("Take Profit (Ticks)"), NumericRange(0, int.MaxValue)]
    public int TakeProfitTicks { get; set; } = 16;

    [Parameter("Stop Loss (Ticks)"), NumericRange(0, int.MaxValue)]
    public int StopLossTicks { get; set; } = 8;

    [Parameter("Trailing Stop")] 
    public bool TrailingStopEnabled { get; set; } = true;

    private IOrder _entryOrder, _takeProfitOrder, _stopLossOrder;
    private int _takeProfitTicks, _stopLossTicks, _ticksInProfit;

    protected override void OnEntryOrder(IOrder order)
    {
        var direction = order.Direction is OrderDirection.Long ? 1 : -1;

        _entryOrder = order;

        if (TakeProfitTicks > 0)
        {
            _takeProfitOrder = SetTakeProfit(order, order.Price + TakeProfitTicks * Symbol.TickSize * direction, "Take Profit");
            _takeProfitTicks = TakeProfitTicks;
        }

        if (StopLossTicks > 0)
        {
            _stopLossOrder = SetStopLoss(_entryOrder, order.Price - StopLossTicks * Symbol.TickSize * direction, "Stop Loss");
            _stopLossTicks = StopLossTicks;
        }
    }

    protected override void OnOrderUpdate(IOrder order)
    {
        var direction = _entryOrder.Direction is OrderDirection.Long ? 1 : -1;

        if (order.Status is OrderStatus.Pending)
        {
            if (order == _entryOrder)
            {
                if (_stopLossOrder?.Status is OrderStatus.Pending)
                {
                    var stopPrice = _entryOrder.Price - _stopLossTicks * Symbol.TickSize * direction;
                    ModifyOrder(_stopLossOrder, _stopLossOrder.Quantity, stopPrice, null);
                }

                if (_takeProfitOrder?.Status is OrderStatus.Pending)
                {
                    var limitPrice = _entryOrder.Price + _takeProfitTicks * Symbol.TickSize * direction;
                    ModifyOrder(_takeProfitOrder, _takeProfitOrder.Quantity, null, limitPrice);
                }
            }
            else if (order == _takeProfitOrder)
            {
                _takeProfitTicks = (int)Math.Round((order.LimitPrice - _entryOrder.Price) / Symbol.TickSize * direction);
            }
            else if (order == _stopLossOrder)
            {
                _stopLossTicks = (int)Math.Round((_entryOrder.Price - order.StopPrice) / Symbol.TickSize * direction);
            }
        }

        if (order == _entryOrder && order.Status is OrderStatus.Cancelled)
        {
            Stop();
        }
        else if (order == _stopLossOrder && order.Status is OrderStatus.Executed)
        {
            Stop();
        }
    }

    protected override void OnPositionUpdate()
    {
        if (_entryOrder.Status is OrderStatus.Executed && Position is null)
        {
            Stop();
        }
    }

    protected override void OnBarUpdate()
    {
        if (TrailingStopEnabled is false)
        {
            return;
        }
        
        if (Position is null || _stopLossOrder?.Status is not OrderStatus.Pending)
        {
            return;
        }

        var direction = _entryOrder.Direction is OrderDirection.Long ? 1 : -1;
        var ticksInProfit = (int)Math.Round((Bars[^1].Close - Position.EntryPrice) / Symbol.TickSize * direction);
        if (ticksInProfit <= _ticksInProfit)
        {
            return;
        }

        var difference = ticksInProfit - _ticksInProfit;

        _stopLossTicks -= difference;
        _ticksInProfit = ticksInProfit;

        var stopPrice = _entryOrder.Price - _stopLossTicks * Symbol.TickSize * direction;

        ModifyOrder(_stopLossOrder, _stopLossOrder.Quantity, stopPrice, null);
    }
}
```

### Parameters
The strategy exposes the following configurable parameters:

1. **Take Profit (Ticks)**  
   - Type: `int`  
   - Description: The number of ticks away from the entry price to set the take profit level.  
   - Default Value: `16`  
   - Range: `0` to `int.MaxValue`  

2. **Stop Loss (Ticks)**  
   - Type: `int`  
   - Description: The number of ticks away from the entry price to set the stop loss level.  
   - Default Value: `8`  
   - Range: `0` to `int.MaxValue`  

3. **Trailing Stop**  
   - Type: `bool`  
   - Description: Enables or disables the trailing stop functionality.  
   - Default Value: `true`  

---

## Fields
- `_entryOrder`: Stores the entry order for the trade.  
- `_takeProfitOrder`: Stores the take profit order.  
- `_stopLossOrder`: Stores the stop loss order.  
- `_takeProfitTicks`: Stores the current take profit distance in ticks.  
- `_stopLossTicks`: Stores the current stop loss distance in ticks.  
- `_ticksInProfit`: Tracks the number of ticks in profit for trailing stop calculations.  

---

## Methods

### `OnEntryOrder(IOrder order)`
- **Description**: Called when an entry order is placed. Initializes the take profit and stop loss orders based on the configured parameters.  
- **Parameters**:  
  - `order` (Type: `IOrder`): The entry order.  
- **Behavior**:  
  - Sets the `_entryOrder` field.  
  - If `TakeProfitTicks` is greater than 0, creates a take profit order.  
  - If `StopLossTicks` is greater than 0, creates a stop loss order.  

---

### `OnOrderUpdate(IOrder order)`
- **Description**: Called when an order is updated. Manages modifications to the stop loss and take profit orders based on the entry order's status.  
- **Parameters**:  
  - `order` (Type: `IOrder`): The updated order.  
- **Behavior**:  
  - Updates the stop loss and take profit orders if the entry order is pending.  
  - Tracks the current take profit and stop loss distances in ticks.  
  - Calls `Stop()` to terminate the strategy if:  
    - The entry order is canceled.  
    - The stop loss order is executed.  

---

### `OnPositionUpdate()`
- **Description**: Called when the position is updated. Stops the strategy if the position is closed.  
- **Behavior**:  
  - Checks if the entry order is executed and the position is null (closed). If true, calls `Stop()` to terminate the strategy.  

---

### `OnBarUpdate()`
- **Description**: Called on every bar update. Implements trailing stop functionality if enabled.  
- **Behavior**:  
  - If `TrailingStopEnabled` is `true`, adjusts the stop loss order to lock in profits as the market moves in favor of the trade.  
  - Calculates the new stop loss price based on the current profit in ticks and modifies the stop loss order accordingly.  

---

### `Stop()`
- **Description**: Terminates the strategy. Once `Stop()` is called, the strategy stops processing updates, and no further logic is executed in the overridden methods (`OnEntryOrder`, `OnOrderUpdate`, `OnPositionUpdate`, `OnBarUpdate`).  
- **Usage**:  
  - Called internally when:  
    - The entry order is canceled.  
    - The stop loss order is executed.  
    - The position is closed.  
  - Can also be called manually to stop the strategy.  

---

## Usage Example
To use the `BasicTradeManagementStrategy`, select the TMS from the `Trade Pad`, configure the parameters and place an order. The strategy will automatically manage the trade by setting and adjusting take profit and stop loss levels.

The strategy remains active and processes updates until the `Stop()` function is called, either internally (e.g., when the stop loss is hit) or manually.
