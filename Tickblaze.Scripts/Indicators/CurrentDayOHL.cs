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

	private Bar _dailyBar;
	private IExchangeSession _lastSession;

	public CurrentDayOHL()
	{
		Name = "Current Day OHL";
		ShortName = "CDOHL";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		if (index == 0)
		{
			return;
		}

		var bar = Bars[index];
		var currentSession = Bars.Symbol.ExchangeCalendar.GetSession(bar.Time);

		var isNewSession = _lastSession is null || _lastSession != currentSession;
		if (isNewSession)
		{
			_lastSession = currentSession;
			_dailyBar = new(currentSession.StartUtcDateTime, bar.Open, bar.High, bar.Low, bar.Close, 0);
		}
		else
		{
			_dailyBar.High = Math.Max(_dailyBar.High, bar.High);
			_dailyBar.Low = Math.Min(_dailyBar.Low, bar.Low);
			_dailyBar.Close = bar.Close;
		}

		Open[index] = _dailyBar.Open;
		High[index] = _dailyBar.High;
		Low[index] = _dailyBar.Low;
	}
}