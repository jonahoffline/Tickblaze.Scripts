using System.ComponentModel;

namespace Tickblaze.Scripts.BarTypes;

[Browsable(false)]
public sealed class RegularBars : BarType
{
	public RegularBars()
	{
		Name = "Regular Bars";
	}

	public override void OnDataPoint(DateTime time, double open, double high, double low, double close, double volume)
	{
		var lastBar = Bars.Count > 0 ? Bars[^1] : null;
		if (lastBar is null || lastBar.Time < time)
		{
			AddBar(time, open, high, low, close, volume);
		}
		else
		{
			UpdateBar(time, open, high, low, close, volume);
		}
	}
}
