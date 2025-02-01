using System.Globalization;

namespace Tickblaze.Scripts.Misc;

public enum VwapResetPeriod
{
	[DisplayName("Day")]
	Day,

	[DisplayName("Week")]
	Week,

	[DisplayName("Month")]
	Month,
}

internal class VwapCalculator(BarSeries bars, ISymbol symbol, VwapResetPeriod? resetPeriod)
{
	private double CumulativeVolume => _closedCumulativeVolume + _curVolume;

	public double Deviation
	{
		get
		{
			var variance = _closedCumulativeVariance + Math.Pow(_curTypical - VWAP, 2) * _curVolume;
			return Math.Sqrt(variance / CumulativeVolume);
		}
	}

	public double VWAP
	{
		get
		{
			var typicalVolume = _closedCumulativeTypicalVolume + _curVolume * _curTypical;
			return typicalVolume / CumulativeVolume;
		}
	}

	private IExchangeSession _currentSession;
	private int _lastCalculateIndex = -1;
	private double _closedCumulativeVolume;
	private double _closedCumulativeTypicalVolume;
	private double _closedCumulativeVariance;

	private double _curVolume;
	private double _curTypical;

	public void Update(int index, out bool isReset)
	{
		isReset = false;

		var bar = bars[index];
		if (bar == null)
		{
			return;
		}

		var typicalPrice = bars.TypicalPrice[index];
		_curVolume = bar.Volume;
		_curTypical = typicalPrice;

		isReset = TryReset(index);

		var prevLastCalculatedIndex = _lastCalculateIndex;
		_lastCalculateIndex = index;

		// On the close of any bar after the first, we append to our closed values
		if (prevLastCalculatedIndex == index || index == 0 || isReset || prevLastCalculatedIndex == -1)
		{
			return;
		}

		var lastBar = bars[index - 1];
		var lastTypical = bars.TypicalPrice[index - 1];
		_closedCumulativeVolume += lastBar.Volume;
		_closedCumulativeTypicalVolume += lastBar.Volume * lastTypical;

		var closedVwap = _closedCumulativeTypicalVolume / _closedCumulativeVolume;
		_closedCumulativeVariance += Math.Pow(lastTypical - closedVwap, 2) * lastBar.Volume;
	}

	private bool TryReset(int barIndex)
	{
		var bar = bars[barIndex];
		var exchangeCalendar = symbol.ExchangeCalendar;
		
		var previousSession = _currentSession;
		
		var currentSession = exchangeCalendar.GetSession(bar.Time);

		_currentSession = currentSession;

		if (previousSession is null
			|| currentSession is null
			|| DateTime.Equals(currentSession.StartUtcDateTime, previousSession.StartUtcDateTime))
		{
			return false;
		}

		var currentTimeUtc = GetMiddleTimeUtc(currentSession);
		var previousTimeUtc = GetMiddleTimeUtc(previousSession);

		var calendar = CultureInfo.InvariantCulture.Calendar;

		var currentWeekNumber = calendar.GetWeekOfYear(currentTimeUtc, CalendarWeekRule.FirstFullWeek, DayOfWeek.Saturday);
		var previousWeekNumber = calendar.GetWeekOfYear(previousTimeUtc, CalendarWeekRule.FirstFullWeek, DayOfWeek.Saturday);
		
		var isResetNeeded = resetPeriod is VwapResetPeriod.Day
			|| resetPeriod is VwapResetPeriod.Week && currentWeekNumber != previousWeekNumber
			|| resetPeriod is VwapResetPeriod.Month && currentTimeUtc.Month != previousTimeUtc.Month;

		if (isResetNeeded)
		{
			_closedCumulativeVolume = 0;
			_closedCumulativeVariance = 0;
			_closedCumulativeTypicalVolume = 0;
		}

		return isResetNeeded;
	}

	private static DateTime GetMiddleTimeUtc(IExchangeSession exchangeSession)
	{
		var deltaTimeSpan = (exchangeSession.EndUtcDateTime - exchangeSession.StartUtcDateTime) / 2.0;

		return exchangeSession.StartUtcDateTime + deltaTimeSpan;
	}
}