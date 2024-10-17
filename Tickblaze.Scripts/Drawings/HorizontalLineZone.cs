namespace Tickblaze.Scripts.Drawings;

public sealed class HorizontalLineZone : HorizontalZone
{
	[Parameter("Line Color")]
	public Color LineColor { get; set; } = Color.White;

	[Parameter("Line Thickness"), NumericRange(1, 10)]
	public int LineThickness { get; set; } = 1;

	[Parameter("Line Style")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Size Type")]
	public SizeType Size { get; set; } = SizeType.Ticks;

	[Parameter("Size Value")]
	public double SizeValue { get; set; } = 5;

	public override int PointsCount => 1;

	public enum SizeType
	{
		Points,
		Ticks
	}

	public HorizontalLineZone()
	{
		Name = "Horizontal LZ";
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		Points[0].Value = Symbol.RoundToTick((double)yDataValue);
	}

	public override void OnRender(IDrawingContext context)
	{
		var midPoint = Points[0];
		var midPrice = (double)midPoint.Value;

		var zoneOffset = SizeValue * (Size is SizeType.Ticks ? Symbol.TickSize : 1);
		var upperPrice = Symbol.RoundToTick(midPrice + zoneOffset);
		var lowerPrice = Symbol.RoundToTick(midPrice - zoneOffset);

		var upperPoint = new ChartPoint()
		{
			Time = midPoint.Time,
			Value = upperPrice,
			X = midPoint.X,
			Y = ChartScale.GetYCoordinateByValue(upperPrice)
		};

		var lowerPoint = new ChartPoint()
		{
			Time = midPoint.Time,
			Value = lowerPrice,
			X = midPoint.X,
			Y = ChartScale.GetYCoordinateByValue(lowerPrice)
		};

		context.DrawExtendedLine(midPoint, new Point(midPoint.X + 10, midPoint.Y), LineColor, LineThickness);
		DrawZone(context, upperPoint, lowerPoint);
	}
}
