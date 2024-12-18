namespace Tickblaze.Scripts.Misc;

internal class VwapCalculator(BarSeries bars, ISymbol symbol)
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

	public void Update(int index)
	{
		var bar = bars[index];
		if (bar == null)
		{
			return;
		}

		var typicalPrice = bars.TypicalPrice[index];
		_curVolume = bar.Volume;
		_curTypical = typicalPrice;

		var currentSession = symbol.ExchangeCalendar.GetSession(bar.Time);
		if (currentSession?.StartExchangeDateTime != _currentSession?.StartExchangeDateTime)
		{
			_currentSession = currentSession;
			_closedCumulativeVolume = 0;
			_closedCumulativeTypicalVolume = 0;
			_closedCumulativeVariance = 0;
		}

		if (_lastCalculateIndex == index)
		{
			return;
		}

		// Bar has ended, calculate the appropriate metric using the last bar and our closed values
		_lastCalculateIndex = index;
		if (index == 0)
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
}