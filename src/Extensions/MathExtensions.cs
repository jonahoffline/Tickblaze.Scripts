namespace Tickblaze.Scripts.Extensions;

internal static class MathExtensions
{
	public static double RoundToNearestMultiple(this double value, double multiple)
	{
		if (multiple == 0)
			throw new ArgumentException("Multiple cannot be zero.", nameof(multiple));

		return Math.Round(value / multiple) * multiple;
	}
	
	public static double FloorToNearestMultiple(this double value, double multiple)
	{
		if (multiple == 0)
			throw new ArgumentException("Multiple cannot be zero.", nameof(multiple));

		return Math.Floor(value / multiple) * multiple;
	}
	
	public static double CielToNearestMultiple(this double value, double multiple)
	{
		if (multiple == 0)
			throw new ArgumentException("Multiple cannot be zero.", nameof(multiple));

		return Math.Ceiling(value / multiple) * multiple;
	}
}
