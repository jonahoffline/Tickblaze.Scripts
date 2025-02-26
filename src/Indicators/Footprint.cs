namespace Tickblaze.Scripts.Indicators;

public partial class Footprint : Indicator
{
	[Parameter("Test")]
	public bool Test { get; set; }

	[Parameter("Value")]
	public double Value { get; set; } = 0.2;
	
	[Parameter("Hello")]
	public string Hello { get; set; } = "Hello";
	
	[Plot("Result")]
	public PlotSeries Result { get; set; }
	
	private double? _barWidth;

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		
		Result[index] = (bar.High + bar.Low) / 2;
	}

	public override void OnRender(IDrawingContext context)
	{
		if (_barWidth is null)
		{
			_barWidth = Chart.BarWidth;

			Chart.BarWidth = 0.25;
		}

		context.DrawLine(new Point(0, 0), new Point(300, 300), Color.Red);
	}

	protected override void OnDestroy()
	{
		if (_barWidth is not null)
		{
			Chart.BarWidth = _barWidth.Value;
		}
	}
}