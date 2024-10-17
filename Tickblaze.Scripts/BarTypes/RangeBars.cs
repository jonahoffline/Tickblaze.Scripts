using System.ComponentModel;

namespace Tickblaze.Scripts.BarTypes;

[Browsable(false)]
public sealed class RangeBars : BarType
{
	public RangeBars()
	{
		Source = SourceDataType.Tick;
	}

	public override void OnDataPoint(DateTime time, double open, double high, double low, double close, double volume)
	{
		if (Bars.Count == 0)
		{
			AddBar(time, open, high, low, close, volume);
		}
		else
		{
			var bar = Bars[^1];
			var maximum = bar.Low + BarSize;
			var minimum = bar.High - BarSize;

			if (close > maximum)
			{
				UpdateBar(bar.Time, bar.Open, maximum, bar.Low, maximum, bar.Volume);
				AddBar(time, close, close, close, close, volume);
			}
			else if (close < minimum)
			{
				UpdateBar(bar.Time, bar.Open, bar.High, minimum, minimum, bar.Volume);
				AddBar(time, close, close, close, close, volume);
			}
			else
			{
				UpdateBar(bar.Time, bar.Open, Math.Max(bar.High, close), Math.Min(bar.Low, close), close, bar.Volume + volume);
			}
		}
	}
}
