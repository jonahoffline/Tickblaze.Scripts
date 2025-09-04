namespace Tickblaze.Scripts.BarTypes;

public sealed class RenkoBxt : BarType
{
	[Parameter("Bar Size (Ticks)", Description = "The base size of a bar")]
	public int BarSize { get; set; } = 4;

	[Parameter("Reversal Size (Ticks)", Description = "The size required to reverse trend. Useful for ignoring smaller pullbacks")]
	public int ReversalSize { get; set; } = 8;

	[Parameter("Open Offset (Ticks)", Description = "The amount to offset the open of every bar against the current trend.")]
	public int Offset { get; set; } = 2;

	private int _trend;

	private IExchangeSession? _currentSession;

	public RenkoBxt()
	{
		Source = SourceDataType.Tick;
		Description = "A custom bar type which uses a standard Renko based construction for trends, and Range based construction for reversals.";
	}

	protected override void OnDataPoint(Bar bar)
	{
		if (Bars.Count == 0 || bar.Time > _currentSession?.EndUtcDateTime)
		{
			_currentSession = Symbol.ExchangeCalendar.GetSession(bar.Time);
			_trend = 0;
			
			AddBar(bar with
			{
				IsComplete = false
			});
			
			return;
		}

		var curLastBar = Bars[^1];
		
		// Handle invalid configurations as one giant bar
		if (Offset >= BarSize || ReversalSize <= Offset)
		{
			UpdateBar(curLastBar with
			{
				High = Math.Max(curLastBar.High, bar.High),
				Low = Math.Min(curLastBar.Low, bar.Low),
				Close = bar.Close,
				Volume = curLastBar.Volume + bar.Volume,
				EndTime = bar.EndTime
			});

			return;
		}

		while (true)
		{
			// Calculate the levels at which a new bar would form
			var trigUp = Bars.Symbol.RoundToTick(_trend == -1 ? curLastBar.Low + ReversalSize * Bars.Symbol.TickSize : curLastBar.Open + BarSize * Bars.Symbol.TickSize);
			var trigDown = Bars.Symbol.RoundToTick(_trend == 1 ? curLastBar.High - ReversalSize * Bars.Symbol.TickSize : curLastBar.Open - BarSize * Bars.Symbol.TickSize);

			// If we haven't moved enough to create a new bar, update the last bar instead
			var direction = bar.Close.CompareTo(trigUp) == 1 ? 1 : bar.Close.CompareTo(trigDown) == -1 ? -1 : 0;
			if (direction == 0)
			{
				var newHigh = Math.Max(curLastBar.High, bar.High);
				var newLow = Math.Min(curLastBar.Low, bar.Low);
				UpdateBar(curLastBar with
				{
					High = newHigh,
					Low = newLow,
					Close = bar.Close,
					Volume = curLastBar.Volume + bar.Volume,
					EndTime = bar.EndTime
				});

				return;
			}

			_trend = direction;

			// Calculate new close and adjust it for tick size
			var newClose = direction == 1 ? trigUp : trigDown;

			// Finalize the current last bar
			UpdateBar(curLastBar with
			{
				High = direction == 1 ? newClose : curLastBar.High,
				Low = direction == 1 ? curLastBar.Low : newClose,
				Close = newClose,
				IsComplete = true,
				EndTime = bar.EndTime,
			});
			
			var newOpen = newClose - direction * Offset * Symbol.TickSize;
			newClose = newOpen + direction * BarSize * Symbol.TickSize;
			if (bar.Close.CompareTo(newClose) == -direction)
			{
				newClose = bar.Close;
			}

			AddBar(curLastBar = new Bar(bar.Time, newOpen, direction == 1 ? newClose : newOpen, direction == 1 ? newOpen : newClose, newClose, 0)
			{
				EndTime = bar.EndTime
			});
		}
	}
}