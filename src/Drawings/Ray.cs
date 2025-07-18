namespace Tickblaze.Scripts.Drawings;

public sealed class Ray : TrendLine
{
	public Ray()
	{
		Name = "Ray";
		SnapToBar = false;
		ExtendRight = true;
	}
}