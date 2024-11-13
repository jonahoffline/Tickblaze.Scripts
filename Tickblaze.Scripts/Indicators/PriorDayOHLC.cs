namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Prior Day OHLC [PDOHLC]
/// </summary>
public partial class PriorDayOHLC : Indicator
{
	[Parameter("Show Open")]
	public bool ShowOpen { get; set; } = true;

	[Parameter("Show High")]
	public bool ShowHigh { get; set; } = true;

	[Parameter("Show Low")]
	public bool ShowLow { get; set; } = true;

	[Parameter("Show Close")]
	public bool ShowClose { get; set; } = true;

	[Plot("Open")]
	public PlotSeries Open { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("High")]
	public PlotSeries High { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("Low")]
	public PlotSeries Low { get; set; } = new(Color.Blue, LineStyle.Dash);

	[Plot("Close")]
	public PlotSeries Close { get; set; } = new(Color.Yellow, LineStyle.Dash);
    
    private BarSeries? _dailyBarSeries;

    public PriorDayOHLC()
	{
		Name = "Prior Day OHLC";
		ShortName = "PDOHLC";
		IsOverlay = true;
	}

    protected override void Initialize()
    {
        _dailyBarSeries = GetBarSeries(new BarSeriesRequest
        {
            Period = new BarPeriod(BarPeriod.SourceType.Day, BarPeriod.PeriodType.Day, 1),
            Symbol = Bars.Symbol,
            BarSeriesContract = new BarSeriesContract { Type = BarSeriesContractType.ContinuousBackAdjusted }
        });
    }

    protected override void Calculate(int index)
	{
        if (_dailyBarSeries is not { Count: > 0 })
            return;

        Open[index] = _dailyBarSeries[^1].Open;
        High[index] = _dailyBarSeries[^1].High;
        Low[index] = _dailyBarSeries[^1].Low;
        Close[index] = _dailyBarSeries[^1].Close;
	}
}