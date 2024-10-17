namespace Tickblaze.Scripts.Drawings;

public abstract class AnchoredVWAP : Drawing
{
	[Parameter("VWAP Color")]
	public Color VWAPLineColor { get; set; } = Color.Blue;

	[Parameter("VWAP Line Thickness"), NumericRange(1, 5)]
	public int VWAPLineThickness { get; set; } = 1;

	[Parameter("Band 1 deviation multiplier"), NumericRange(0, double.MaxValue)]
	public double Band1Mult { get; set; } = 1.0;

	[Parameter("Band 1 Color")]
	public Color Band1Color { get; set; } = Color.Green;

	[Parameter("Band 1 Line Thickness"), NumericRange(1, 5)]
	public int Band1LineThickness { get; set; } = 1;

	[Parameter("Band 2 deviation multiplier"), NumericRange(0, double.MaxValue)]
	public double Band2Mult { get; set; } = 2.0;

	[Parameter("Band 2 Color")]
	public Color Band2Color { get; set; } = Color.Yellow;

	[Parameter("Band 2 Line Thickness"), NumericRange(1, 5)]
	public int Band2LineThickness { get; set; } = 1;

	[Parameter("Band 3 deviation multiplier"), NumericRange(0, double.MaxValue)]
	public double Band3Mult { get; set; } = 3.0;

	[Parameter("Band 3 Color")]
	public Color Band3Color { get; set; } = Color.Red;

	[Parameter("Band 3 Line Thickness"), NumericRange(1, 5)]
	public int Band3LineThickness { get; set; } = 1;

	public override int PointsCount => 2;

	//The Key is the Plot number (1 is Band1, 2 is Band2, 3 is Band3)
	//Value is double[3], 0 element is the deviation multiplier, 1st element is the upper plot line prior Y pixel value, and 2nd element is the lower plot line prior Y pixel value
	private Dictionary<int, double[]> _plotData = [];
	private Dictionary<int, Color> _bandColors = [];
	private Dictionary<int, int> _bandThicknesses = [];

	public AnchoredVWAP()
	{
		Name = "Anchored VWAP";
	}

	public override void OnRender(IDrawingContext context)
	{
		var leftIndex = Chart.GetBarIndexByXCoordinate(Math.Min(Points[0].X, Points[1].X));
		var rightIndex = Chart.GetBarIndexByXCoordinate(Math.Max(Points[0].X, Points[1].X));

		var volumeSum = 0.0;
		var typicalVolumeSum = 0.0;
		var varianceSum = 0.0;
		var pointL = new Point(0, 0);
		var pointR = new Point(0, 0);

		_plotData[0] = [Band1Mult, 0, 0];
		_bandColors[0] = VWAPLineColor;
		_bandThicknesses[0] = VWAPLineThickness;
		if (Band1Mult > 0)
		{
			_plotData[1] = [Band1Mult, 0, 0];
			_bandColors[1] = Band1Color;
			_bandThicknesses[1] = Band1LineThickness;
		}

		if (Band2Mult > 0)
		{
			_plotData[2] = [Band2Mult, 0, 0];
			_bandColors[2] = Band2Color;
			_bandThicknesses[2] = Band2LineThickness;
		}

		if (Band3Mult > 0)
		{
			_plotData[3] = [Band3Mult, 0, 0];
			_bandColors[3] = Band3Color;
			_bandThicknesses[3] = Band3LineThickness;
		}

		for (var i = leftIndex; i >= rightIndex; i++)
		{
			volumeSum += Bars[i].Volume;
			typicalVolumeSum += Bars[i].Volume * Bars.TypicalPrice[i];

			if (i == leftIndex)
			{
				pointR = new Point(Chart.GetXCoordinateByBarIndex(i), ChartScale.GetYCoordinateByValue(Bars.TypicalPrice[i]));
				foreach (var kvp in _plotData)
				{
					//left-edge of all plots start out at the same Y pixel
					kvp.Value[0] = kvp.Value[1] = kvp.Value[2] = kvp.Value[3] = pointR.Y;
				}
			}
			else
			{
				var curVWAP = typicalVolumeSum / volumeSum;
				var diff = Bars.TypicalPrice[i] - curVWAP;
				varianceSum += diff * diff;
				var deviation = Math.Sqrt(Math.Max(varianceSum / (i - leftIndex), 0));

				//left-edge X pixel is set to the last print X pixel
				pointL.X = pointR.X;
				pointR.X = Chart.GetXCoordinateByBarIndex(i);
				foreach (var kvp in _plotData)
				{
					//kvp.Value[0] element of is the band multiplier
					//kvp.Value[1] element is the upper line prior Y-pixel value
					pointL.Y = kvp.Value[1];
					pointR.Y = ChartScale.GetValueByYCoordinate(curVWAP + deviation * kvp.Value[0]);
					kvp.Value[1] = pointR.Y;
					context.DrawLine(pointL, pointR, _bandColors[kvp.Key], _bandThicknesses[kvp.Key]);

					//the VWAP line is key of 0, draw the lower line plot only if this is band 1, 2 or 3
					if (kvp.Key != 0)
					{
						//kvp.Value[2] element is the lower line prior Y-pixel value
						pointL.Y = kvp.Value[2];
						pointR.Y = ChartScale.GetValueByYCoordinate(curVWAP - deviation * kvp.Value[0]);
						kvp.Value[2] = pointR.Y;
						context.DrawLine(pointL, pointR, _bandColors[kvp.Key], _bandThicknesses[kvp.Key]);
					}
				}
			}
		}
	}
}