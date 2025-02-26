namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// CurrentDayOHL [CDOHL]
/// </summary>
public partial class CurrentDayOHL : Indicator
{
	[Plot("Open")]
	public PlotSeries Open { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("High")]
	public PlotSeries High { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("Low")]
	public PlotSeries Low { get; set; } = new(Color.Blue, LineStyle.Dash);

	private bool _isDailyChart;
	private IExchangeSession _lastSession;
	private double _open, _high, _low;

	public CurrentDayOHL()
	{
		Name = "Current Day OHL";
		ShortName = "CDOHL";
		IsOverlay = true;
	}
	protected override void Initialize()
	{
		_isDailyChart = Bars.Period.Source is BarPeriod.SourceType.Day;
	}

	protected override void Calculate(int index)
	{
		if (_isDailyChart)
		{
			return;
		}

		var bar = Bars[index];
		var session = Bars.Symbol.ExchangeCalendar.GetSession(bar.Time);

		if (session != _lastSession)
		{
			_lastSession = session;
			_open = bar.Open;
			_high = bar.High;
			_low = bar.Low;
			Open.IsLineBreak[index] = High.IsLineBreak[index] = Low.IsLineBreak[index] = true;
		}
		else
		{
			_high = Math.Max(_high, bar.High);
			_low = Math.Min(_low, bar.Low);
		}

		Open[index] = _open;
		High[index] = _high;
		Low[index] = _low;
	}
}