namespace Tickblaze.Scripts.Drawings;

public abstract class Line : Drawing
{
	[Parameter("Color")]
	public Color Color { get; set; } = "#389d44";

	[Parameter("Thickness"), NumericRange(1, 5)]
	public int Thickness { get; set; } = 1;

	[Parameter("Line Style")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => 2;

	public IChartPoint PointA => Points[0];
	public IChartPoint PointB => Points[1];
}
