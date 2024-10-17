namespace Tickblaze.Scripts.Drawings;

public partial class HorizontalZone : Drawing
{
	[Parameter("Risk $")]
	public double RiskValue { get; set; } = 500;

	[Parameter("Fill")]
	public Color FillColor { get; set; } = "#33ebc800";

	[Parameter("Outline color")]
	public Color OutlineColor { get; set; } = "#ebc800";

	[Parameter("Outline thickness"), NumericRange(1)]
	public int OutlineThickness { get; set; } = 1;

	[Parameter("Outline style")]
	public LineStyle OutlineLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Text Font")]
	public Font TextFont { get; set; } = new("Arial", 10);

	public override int PointsCount => 2;

	public HorizontalZone()
	{
		Name = "Horizontal Zone";
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		Points[index].Value = Symbol.RoundToTick((double)yDataValue);
		Points[Math.Abs(index - 1)].Time = xDataValue;
	}

	public override void OnRender(IDrawingContext context)
	{
		var pointA = Points[0].Y < Points[1].Y ? Points[0] : Points[1];
		var pointB = Points[0].Y < Points[1].Y ? Points[1] : Points[0];

		DrawZone(context, pointA, pointB);
	}

	protected void DrawZone(IDrawingContext context, IChartPoint upperPoint, IChartPoint lowerPoint)
	{
		var x = context.RenderSize.Width + OutlineThickness;
		var points = new IPoint[]
		{
			upperPoint,
			new Point(x, upperPoint.Y),
			new Point(x, lowerPoint.Y),
			lowerPoint,
			upperPoint,
		};

		context.DrawPolygon(points, FillColor, OutlineColor, OutlineThickness, OutlineLineStyle);

		var upperPrice = (double)upperPoint.Value;
		var lowerPrice = (double)lowerPoint.Value;
		var ticks = (int)Math.Round((upperPrice - lowerPrice) / Symbol.TickSize);
		var quantity = Math.Max(0, Symbol.NormalizeVolume(RiskValue / (ticks * Symbol.TickValue), RoundingMode.Up));
		var text = $"{quantity} @ {Symbol.FormatPrice(upperPrice)}";
		var textSize = context.MeasureText(text, TextFont);

		context.DrawText(new Point(upperPoint.X, upperPoint.Y - textSize.Height), text, OutlineColor, TextFont);
		context.DrawText(lowerPoint, $"{quantity} @ {Symbol.FormatPrice(lowerPrice)}", OutlineColor, TextFont);
	}
}