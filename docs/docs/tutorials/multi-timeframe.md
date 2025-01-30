# Accessing Secondary Data

In this tutorial, you will learn how to access secondary data (e.g., bars from different timeframes) using the `GetBars()` method in Tickblaze. This is particularly useful for creating multi-timeframe (MTF) indicators, where you need to analyze data from multiple timeframes simultaneously.

We will use the `MtfMovingAverage` example provided to demonstrate how to retrieve bars from different timeframes (1-minute, 5-minute, and 15-minute) and calculate moving averages for each timeframe.

---

## Step 1: Create the Indicator Class
Start by creating a new indicator class that inherits from the `Indicator` base class. Define the necessary properties and plot series for the moving averages.

```csharp
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Tests;

public class MtfMovingAverage : Indicator
{
    // Plot series for 1-minute SMA
    [Plot("SMA Minute-1")]
    public PlotSeries SMA1 { get; set; } = new PlotSeries(Color.Red);

    // Plot series for 5-minute SMA
    [Plot("SMA Minute-5")]
    public PlotSeries SMA5 { get; set; } = new PlotSeries(Color.Green);

    // Plot series for 15-minute SMA
    [Plot("SMA Minute-15")]
    public PlotSeries SMA15 { get; set; } = new PlotSeries(Color.Blue);

    // Bar series for different timeframes
    private BarSeries _minute1, _minute5, _minute15;

    // Moving average indicators for each timeframe
    private SimpleMovingAverage _sma1, _sma5, _sma15;
}
```

---

## Step 2: Initialize Secondary Data in the `Initialize()` Method
The `Initialize()` method is where you set up the secondary data series using the `GetBars()` method. This method allows you to request bars for a specific symbol and timeframe.

```csharp
protected override void Initialize()
{
    // Create a BarSeriesRequest object with the current symbol and contract settings
    var barSeriesRequest = new BarSeriesRequest
    {
        SymbolCode = Bars.Symbol.Code,
        Exchange = Bars.Symbol.Exchange,
        InstrumentType = Bars.Symbol.Type,
        Contract = Bars.ContractSettings,
        IsETH = Bars.IsETH
    };

    // Request 1-minute bars
    _minute1 = GetBars(barSeriesRequest with
    {
        Period = new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 1)
    });

    // Request 5-minute bars
    _minute5 = GetBars(barSeriesRequest with
    {
        Period = new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 5)
    });

    // Request 15-minute bars
    _minute15 = GetBars(barSeriesRequest with
    {
        Period = new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 15)
    });

    // Initialize moving averages for each timeframe
    _sma1 = new SimpleMovingAverage(_minute1.Close, 20) { Bars = _minute1 };
    _sma5 = new SimpleMovingAverage(_minute5.Close, 20) { Bars = _minute5 };
    _sma15 = new SimpleMovingAverage(_minute15.Close, 20) { Bars = _minute15 };
}
```

### Explanation:
- **`BarSeriesRequest`**: This object specifies the symbol, exchange, instrument type, contract settings, and session type (ETH or RTH) for the requested bars.
- **`GetBars()`**: This method retrieves bars for the specified timeframe. The `BarPeriod` object defines the timeframe (e.g., 1-minute, 5-minute, 15-minute).
- **`SimpleMovingAverage`**: A built-in indicator that calculates the simple moving average (SMA) for the specified bar series.

---

## Step 3: Calculate the Moving Averages in the `Calculate()` Method
The `Calculate()` method is called for each bar in the primary series (the series the indicator is applied to). Here, we calculate the moving averages for each secondary timeframe and assign them to the corresponding plot series.

```csharp
protected override void Calculate(int index)
{
    // Calculate and assign the 1-minute SMA
    if (_minute1.Count > 0)
    {
        SMA1[index] = _sma1[_minute1.Count - 1];
    }

    // Calculate and assign the 5-minute SMA
    if (_minute5.Count > 0)
    {
        SMA5[index] = _sma5[_minute5.Count - 1];
    }

    // Calculate and assign the 15-minute SMA
    if (_minute15.Count > 0)
    {
        SMA15[index] = _sma15[_minute15.Count - 1];
    }
}
```

### Explanation:
- **`_minute1.Count > 0`**: Ensures that there are bars available in the secondary series before attempting to access them.
- **`_sma1[_minute1.Count - 1]`**: Retrieves the most recent value of the 1-minute SMA.
- The same logic is applied to the 5-minute and 15-minute SMAs.

---

## Step 4: Complete Indicator Code
Here is the complete code for the `MtfMovingAverage` indicator:

```csharp
using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Tests;

public class MtfMovingAverage : Indicator
{
    [Plot("SMA Minute-1")]
    public PlotSeries SMA1 { get; set; } = new PlotSeries(Color.Red);

    [Plot("SMA Minute-5")]
    public PlotSeries SMA5 { get; set; } = new PlotSeries(Color.Green);

    [Plot("SMA Minute-15")]
    public PlotSeries SMA15 { get; set; } = new PlotSeries(Color.Blue);

    private BarSeries _minute1, _minute5, _minute15;
    private SimpleMovingAverage _sma1, _sma5, _sma15;

    protected override void Initialize()
    {
        var barSeriesRequest = new BarSeriesRequest
        {
            SymbolCode = Bars.Symbol.Code,
            Exchange = Bars.Symbol.Exchange,
            InstrumentType = Bars.Symbol.Type,
            Contract = Bars.ContractSettings,
            IsETH = Bars.IsETH
        };

        _minute1 = GetBars(barSeriesRequest with
        {
            Period = new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 1)
        });

        _minute5 = GetBars(barSeriesRequest with
        {
            Period = new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 5)
        });

        _minute15 = GetBars(barSeriesRequest with
        {
            Period = new BarPeriod(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 15)
        });

        _sma1 = new SimpleMovingAverage(_minute1.Close, 20) { Bars = _minute1 };
        _sma5 = new SimpleMovingAverage(_minute5.Close, 20) { Bars = _minute5 };
        _sma15 = new SimpleMovingAverage(_minute15.Close, 20) { Bars = _minute15 };
    }

    protected override void Calculate(int index)
    {
        if (_minute1.Count > 0)
        {
            SMA1[index] = _sma1[_minute1.Count - 1];
        }

        if (_minute5.Count > 0)
        {
            SMA5[index] = _sma5[_minute5.Count - 1];
        }

        if (_minute15.Count > 0)
        {
            SMA15[index] = _sma15[_minute15.Count - 1];
        }
    }
}
```

---

## Step 5: Using the Indicator
1. Compile the script and ensure there are no errors.
2. Add the `MtfMovingAverage` indicator to your chart.
3. Observe the moving averages for the 1-minute, 5-minute, and 15-minute timeframes plotted on the chart.

---

## Conclusion
This tutorial demonstrated how to access secondary data using the `GetBars()` method and create a multi-timeframe indicator. By leveraging this approach, you can build powerful indicators that analyze data across multiple timeframes simultaneously.
