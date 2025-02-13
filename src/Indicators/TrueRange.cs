namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// True Range [TR]
/// </summary>
public partial class TrueRange : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; }

	public TrueRange()
	{
		Name = "True Range";
		ShortName = "TR";
	}

	protected override void Calculate(int index)
	{
		var currentLow = Bars.Low[index];
		var currentHigh = Bars.High[index];

		var trueRange = currentHigh - currentLow;

		if (index is not 0)
		{
			var previousClose = Bars.Close[index - 1];
			
			trueRange = Math.Max(trueRange, Math.Abs(currentLow - previousClose));
			trueRange = Math.Max(trueRange, Math.Abs(currentHigh - previousClose));
		}
		
		Result[index] = trueRange;
	}
}
