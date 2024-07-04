using Tickblaze.API;

namespace Tickblaze.Scripts.Indicators;

public sealed class SimpleMovingAverage : Indicator
{
	[Parameter("Period", Minimum = 1, Maximum = 999, Step = 1)]
	public int Period { get; set; } = 20;

	[Parameter("Test")]
	public bool IsChecked { get; set; }

	[Plot("Result", "#2962ff", LineStyle.Solid, 1)]
	public PlotSeries Result { get; set; }

	[Plot("TEST", "#f00", LineStyle.Solid, 1)]
	public PlotSeries Test { get; set; }

	public override void Calculate(int index)
	{
		var period = Math.Min(Period, index + 1);
		var sum = 0.0;

		for (var i = 0; i < period; i++)
			sum += Bars[index - i].Close;

		Result[index] = sum / period;
		Test[index] = Result[index] + 10;
	}
}
