namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Prior Day OHLC [PDOHLC]
/// </summary>
public partial class PriorDayOHLC : Indicator
{
	[Plot("Open")]
	public PlotSeries Open { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("High")]
	public PlotSeries High { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("Low")]
	public PlotSeries Low { get; set; } = new(Color.Blue, LineStyle.Dash);

	[Plot("Close")]
	public PlotSeries Close { get; set; } = new(Color.Gray, LineStyle.Dash);

	private bool _isDailyChart;
	private IExchangeSession _lastSession;
	private double _open, _high, _low, _close;

	public PriorDayOHLC()
	{
		Name = "Prior Day OHLC";
		ShortName = "PDOHLC";
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

		if (index > 0 && double.IsNaN(Open[index]))
		{
			Open[index] = Open[index - 1];
			High[index] = High[index - 1];
			Low[index] = Low[index - 1];
			Close[index] = Close[index - 1];
		}

		var bar = Bars[index];
		var currentSession = Bars.Symbol.ExchangeCalendar.GetSession(bar.Time);

		var isNewSession = _lastSession is null || _lastSession != currentSession;
		if (isNewSession)
		{
			if (_lastSession is not null)
			{
				Open[index] = _open;
				High[index] = _high;
				Low[index] = _low;
				Close[index] = _close;

				Open.IsLineBreak[index] = High.IsLineBreak[index] = Low.IsLineBreak[index] = Close.IsLineBreak[index] = true;
			}

			_lastSession = currentSession;
			_open = bar.Open;
			_high = bar.High;
			_low = bar.Low;
			_close = bar.Close;
		}
		else
		{
			_high = Math.Max(_high, bar.High);
			_low = Math.Min(_low, bar.Low);
			_close = bar.Close;
		}
	}
}