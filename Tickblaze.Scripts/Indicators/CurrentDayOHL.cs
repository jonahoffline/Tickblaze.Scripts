namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// CurrentDayOHL [CDOHL]
/// </summary>
public class CurrentDayOHL : Indicator
{
	[Plot("Open")]
	public PlotSeries Open { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("High")]
	public PlotSeries High { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("Low")]
	public PlotSeries Low { get; set; } = new(Color.Blue, LineStyle.Dash);

	private double _highestHigh = double.MinValue;
	private double _lowestLow = double.MaxValue;
    private BarSeries? _dailyBarSeries;
    private int _lastDailyBarCount = -1;
    private double _currentDayOpen;
	public CurrentDayOHL()
	{
		Name = "Current Day OHL";
		ShortName = "CDOHL";
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
        if (_dailyBarSeries is not { Count: >= 0 })
            return;

        if (_lastDailyBarCount != _dailyBarSeries.Count)
        {
            _highestHigh = double.MinValue;
            _lowestLow = double.MaxValue;
            _currentDayOpen = Bars.Open[index];
            _lastDailyBarCount = _dailyBarSeries.Count;
        }

        _highestHigh = Math.Max(_highestHigh, Bars.High[index]);
        _lowestLow = Math.Min(_lowestLow, Bars.Low[index]);

        Open[index] = _currentDayOpen;
        High[index] = _highestHigh;
        Low[index] = _lowestLow;
	}
}