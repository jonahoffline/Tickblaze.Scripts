namespace Tickblaze.Scripts.Drawings;

public sealed class Triangle : Drawing
{
	[Parameter("Background")]
	public Color BackgroundColor { get; set; } = "#339c27b0";

	[Parameter("Border color")]
	public Color BorderColor { get; set; } = "#9c27b0";

	[Parameter("Border thickness"), NumericRange(1)]
	public int BorderThickness { get; set; } = 1;

	[Parameter("Border line style")]
	public LineStyle BorderLineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => 3;

	public Triangle()
	{
		Name = "Triangle";
	}

	public override void OnRender(IDrawingContext context)
	{
		var points = Points.ToArray();

		if (points.Length == PointsCount)
		{
			Array.Resize(ref points, points.Length + 1);
			points[^1] = points[0];
		}

		context.DrawPolygon(points, BackgroundColor, BorderColor, BorderThickness, BorderLineStyle);
	}
}
