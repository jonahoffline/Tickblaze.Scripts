namespace Tickblaze.Scripts.Drawings;

public abstract class ArrowBase : Drawing
{
	[Parameter("Color", Description = "Arrow fill color")]
	public Color Color { get; set; } = Color.Red;

	[Parameter("Size", Description = "Arrow size, from 6 to 100"), NumericRange(6, 100)]
	public int Size { get; set; } = 18;

	protected ArrowType Type { get; init; }

	protected enum ArrowType
	{
		Up,
		Down
	}

	public override int PointsCount => 1;

	public override void OnRender(IDrawingContext context)
	{
		context.DrawArrow(new ArrowInfo(Size, Math.PI * (Type == ArrowType.Up ? 3 : 1) / 2, Points[0], Size / 2d), Color, new Stroke { Color = Color.Transparent });
	}
}
