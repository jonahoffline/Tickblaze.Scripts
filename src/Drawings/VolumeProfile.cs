
using System.Diagnostics;

namespace Tickblaze.Scripts.Drawings;

public class VolumeProfileExtended : VolumeProfile
{
	protected override bool ExtendRight => true;

	public VolumeProfileExtended()
	{
		Name = "Volume Profile - Realtime";
	}
}

public class VolumeProfile : Drawing
{
	[Parameter("Source Data", Description = "Data source for the volume profile")]
	public SourceDataType SourceData { get; set; } = SourceDataType.Chart;

	public const string StyleGroupName = "style";

	[Parameter("Histo Size Type", Description = "Determines how histogram rows are calculated (by count or ticks)")]
	public RowsLayoutType RowsLayout { get; set; } = RowsLayoutType.Count;

	[NumericRange(1, int.MaxValue)]
	[Parameter("Histo Size Value", Description = "Defines the size of the histogram rows")]
	public int RowsSize { get; set; } = 24;

	[NumericRange(0, 100)]
	[Parameter("Histo Width %", Description = "Width of the histogram as a percentage of box width", GroupName = StyleGroupName)]
	public double RowsWidthPercent { get; set; } = 30;

	[Parameter("Histo Location", Description = "Location of histogram (left or right side of box)", GroupName = StyleGroupName)]
	public PlacementType RowsPlacement { get; set; } = PlacementType.Left;

	[NumericRange(0, 100)]
	[Parameter("Value Area %", Description = "Percentage of total volume considered in the value area", GroupName = StyleGroupName)]
	public double ValueAreaPercent { get; set; } = 70;

	[Parameter("Value Area Color", Description = "Color of the value area", GroupName = StyleGroupName)]
	public Color ValueAreaColor { get; set; } = "#bf808080";

	[Parameter("Above Value Area Color", Description = "Color of area above the value area", GroupName = StyleGroupName)]
	public Color ValueAreaAboveColor { get; set; } = "#80ff0000";

	[Parameter("Below Value Area Color", Description = "Color of area below the value area", GroupName = StyleGroupName)]
	public Color ValueAreaBelowColor { get; set; } = "#80ff0000";

	[Parameter("Outline Color", Description = "Color of the volume profile outline box", GroupName = StyleGroupName)]
	public Color BoxLineColor { get; set; } = "#80ffffff";

	[Parameter("Outline Thickness", Description = "Thickness of the volume profile outline box", GroupName = StyleGroupName)]
	public int BoxLineThickness { get; set; } = 1;

	[Parameter("Outline Style", Description = "Style of the volume profile outline box (solid, dashed, etc.)", GroupName = StyleGroupName)]
	public LineStyle BoxLineStyle { get; set; } = LineStyle.Dot;

	[Parameter("VAH Line Visible?", Description = "Show/Hide the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public bool VahLineVisible { get; set; } = false;

	[Parameter("VAH Line Color", Description = "Color of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public Color VahLineColor { get; set; } = Color.White;

	[Parameter("VAH Line Thickness", Description = "Thickness of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public int VahLineThickness { get; set; } = 2;

	[Parameter("VAH Line Style", Description = "Style of the Value Area High (VAH) line", GroupName = StyleGroupName)]
	public LineStyle VahLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("VAL Line Visible?", Description = "Show/Hide the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public bool ValLineVisible { get; set; } = false;

	[Parameter("VAL Line Color", Description = "Color of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public Color ValLineColor { get; set; } = Color.White;

	[Parameter("VAL Line Thickness", Description = "Thickness of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public int ValLineThickness { get; set; } = 2;

	[Parameter("VAL Line Style", Description = "Style of the Value Area Low (VAL) line", GroupName = StyleGroupName)]
	public LineStyle ValLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("POC Line Visible?", Description = "Show/Hide the Point of Control (POC) line", GroupName = StyleGroupName)]
	public bool PocLineVisible { get; set; } = true;

	[Parameter("POC Line Color", Description = "Color of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public Color PocLineColor { get; set; } = Color.Yellow;

	[Parameter("POC Line Thickness", Description = "Thickness of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public int PocLineThickness { get; set; } = 2;

	[Parameter("POC Line Style", Description = "Style of the Point of Control (POC) line", GroupName = StyleGroupName)]
	public LineStyle PocLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Show VAH/VAL/POC Prices?", Description = "Displays prices for VAH, VAL, and POC levels", GroupName = StyleGroupName)]
	public bool ShowPrices { get; set; } = false;

	[Parameter("Font", Description = "Font for displaying VAH/VAL/POC prices", GroupName = StyleGroupName)]
	public Font Font { get; set; } = new("Arial", 12);

	[Parameter("VWAP Enabled?", Description = "Show/Hide the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public bool VwapEnabled { get; set; } = false;

	[Parameter("VWAP Line Color", Description = "Color of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public Color VwapLineColor { get; set; } = Color.Blue;

	[Parameter("VWAP Line Thickness", Description = "Thickness of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public int VwapLineThickness { get; set; } = 1;

	[Parameter("VWAP Line Style", Description = "Style of the Volume Weighted Average Price (VWAP) line", GroupName = StyleGroupName)]
	public LineStyle VwapLineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => ExtendRight ? 1 : 2;

	protected virtual bool ExtendRight => false;

	public enum SourceDataType
	{
		Chart,
		Minute,
		Second,
		Tick
	}

	public enum RowsLayoutType
	{
		Count,
		Ticks
	}

	public enum PlacementType
	{
		Left,
		Right
	}

	private record Area
	{
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
		public double High { get; set; } = double.MinValue;
		public double Low { get; set; } = double.MaxValue;
		public double Maximum { get; set; }
		public double Minimum { get; set; }
		public double Volume { get; set; }
		public double Range => High - Low;
		public double RowSize { get; set; }
		public int Rows { get; set; }
		public bool IsTickSize { get; set; }
		public BarsRange Bars { get; set; } = null;
	}

	private record BarsRange
	{
		public int FromIndex { get; set; }
		public int ToIndex { get; set; }
	}

	private record Volume()
	{
		public double Buy { get; set; }
		public double Sell { get; set; }
		public double Total => Buy + Sell;
		public double Delta => Buy - Sell;

		public static implicit operator double(Volume volume) => volume.Total;
	}

	private BarSeries _bars;
	private Area _area;
	private Volume[] _volumes;
	private int _pocIndex, _vahIndex, _valIndex;
	private double[] _vwap;

	public VolumeProfile()
	{
		Name = "Volume Profile - Manual";
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		parameters[nameof(RowsSize)].Attributes.Name = RowsLayout is RowsLayoutType.Count ? "Histo Rows Count" : "Histo Ticks Size";

		if (VahLineVisible is false)
		{
			parameters.Remove(nameof(VahLineColor));
			parameters.Remove(nameof(VahLineThickness));
			parameters.Remove(nameof(VahLineStyle));
		}

		if (ValLineVisible is false)
		{
			parameters.Remove(nameof(ValLineColor));
			parameters.Remove(nameof(ValLineThickness));
			parameters.Remove(nameof(ValLineStyle));
		}

		if (PocLineVisible is false)
		{
			parameters.Remove(nameof(PocLineColor));
			parameters.Remove(nameof(PocLineThickness));
			parameters.Remove(nameof(PocLineStyle));
		}

		if (VwapEnabled is false)
		{
			parameters.Remove(nameof(VwapLineColor));
			parameters.Remove(nameof(VwapLineThickness));
			parameters.Remove(nameof(VwapLineStyle));
		}

		if (ShowPrices is false)
		{
			parameters.Remove(nameof(Font));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		var barPeriod = SourceData switch
		{
			SourceDataType.Chart => Bars.Period,
			SourceDataType.Minute => new BarPeriod(BarPeriod.SourceType.Trade, BarPeriod.PeriodType.Minute, 1),
			SourceDataType.Second => new BarPeriod(BarPeriod.SourceType.Trade, BarPeriod.PeriodType.Second, 1),
			SourceDataType.Tick => new BarPeriod(BarPeriod.SourceType.Trade, BarPeriod.PeriodType.Tick, 1),
			_ => throw new NotImplementedException()
		};

		_bars = SourceData is SourceDataType.Chart ? Bars : GetBarSeries(new()
		{
			Period = barPeriod,
			SymbolCode = Bars.Symbol.Code,
			Exchange = Bars.Symbol.Exchange,
			InstrumentType = Bars.Symbol.Type,
			Contract = new ContractSettings
			{
				Type = ContractType.ContinuousByDataProvider
			},
			IsETH = Bars.IsETH
		});
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count >= PointsCount)
		{
			AdjustAnchorPoints(CalculateArea());
		}
	}

	private void AdjustAnchorPoints(Area area)
	{
		var midPrice = (area.High + area.Low) / 2;

		foreach (var point in Points)
		{
			if (point.Value.Equals(midPrice) is false)
			{
				point.Value = midPrice;
			}
		}

		var firstPoint = Points.OrderBy(x => x.X).First();
		var lastBarTime = Chart.GetTimeByXCoordinate(Chart.GetXCoordinateByBarIndex(Bars.Count - 1));

		if ((DateTime)firstPoint.Time > lastBarTime)
		{
			firstPoint.Time = lastBarTime;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		var area = CalculateArea();

		if (_area is null || _area != area)
		{
			_area = area;

			AdjustAnchorPoints(area);
			CalculateProfile(area);
			CalculateVwap(area);
		}

		Render(context);
	}

	private Area CalculateArea()
	{
		var area = new Area()
		{
			FromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X),
			ToIndex = ExtendRight ? Bars.Count - 1 : Chart.GetBarIndexByXCoordinate(Points[1].X)
		};

		if (area.FromIndex > area.ToIndex)
		{
			(area.FromIndex, area.ToIndex) = (area.ToIndex, area.FromIndex);
		}

		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];

			if (area.High < bar.High)
			{
				area.High = bar.High;
			}

			if (area.Low > bar.Low)
			{
				area.Low = bar.Low;
			}

			area.Volume += bar.Volume;
		}

		// Bar time offset by 1, temp fix for open time
		var barOffset = SourceData is SourceDataType.Chart ? 0 : 1;
		var fromTime = Bars[Math.Min(Bars.Count - 1, area.FromIndex + barOffset)].Time;
		var toTime = area.ToIndex >= Bars.Count - (1 + barOffset) ? DateTime.MaxValue : Bars[area.ToIndex + (1 + barOffset)].Time;

		for (var i = 0; i < _bars.Count; i++)
		{
			var bar = _bars[i];
			if (bar is null)
			{
				continue;
			}

			var barTime = bar.Time;
			if (barTime < fromTime)
			{
				continue;
			}

			if (barTime < toTime)
			{
				area.Bars ??= new() { FromIndex = i };
				area.Bars.ToIndex = i;
			}
			else
			{
				break;
			}
		}

		if (RowsLayout is RowsLayoutType.Ticks)
		{
			area.RowSize = Symbol.TickSize * RowsSize;
			area.IsTickSize = true;
		}
		else
		{
			area.RowSize = area.Range / RowsSize;

			if (area.RowSize <= Symbol.TickSize)
			{
				area.RowSize = Symbol.TickSize;
				area.IsTickSize = true;
			}
		}

		var rows = (int)Math.Round(area.Range / area.RowSize);
		var rowsMaximum = 500;

		if (rows > rowsMaximum)
		{
			area.RowSize = area.Range / rowsMaximum;

			if (area.IsTickSize)
			{
				area.RowSize = Symbol.RoundToTick(area.RowSize);
			}
		}

		if (area.RowSize <= 0)
		{
			area.Rows = 0;
		}
		else if (RowsLayout is RowsLayoutType.Ticks)
		{
			area.Minimum = area.Low;// - Symbol.TickSize / 2;
			area.Maximum = area.Minimum + area.RowSize * rows;
			area.Rows = rows + 1;
		}
		else
		{
			area.Maximum = area.High;
			area.Minimum = area.Low;
			area.Rows = rows;
		}

		return area;
	}

	private void CalculateProfile(Area area)
	{
		if (area.Rows <= 0 || area.Bars is null)
		{
			return;
		}

		_volumes = new Volume[area.Rows];

		for (var index = area.Bars.FromIndex; index <= _area.Bars.ToIndex; index++)
		{
			var bar = _bars[index];
			var buyVolume = 0.0;
			var sellVolume = 0.0;

			if (SourceData is SourceDataType.Tick)
			{
				if (index > 0)
				{
					var currentPrice = bar.Close;
					var previousPrice = _bars[index - 1].Close;

					if (currentPrice > previousPrice)
					{
						buyVolume = bar.Volume;
					}
					else if (currentPrice < previousPrice)
					{
						sellVolume = bar.Volume;
					}
					else
					{
						buyVolume = sellVolume = bar.Volume / 2;
					}
				}
				else
				{
					buyVolume = sellVolume = bar.Volume / 2;
				}

				var level = (int)Math.Floor((bar.Close - area.Minimum) / area.RowSize);
				level = Math.Max(0, Math.Min(_volumes.Length - 1, level));

				_volumes[level] ??= new();
				_volumes[level].Buy += buyVolume;
				_volumes[level].Sell += sellVolume;
			}
			else
			{
				int startLevel, endLevel;

				if (bar.High == bar.Low)
				{
					startLevel = endLevel = Math.Max(0, Math.Min(_volumes.Length - 1, (int)Math.Floor((bar.High - area.Minimum) / area.RowSize)));
				}
				else
				{
					startLevel = Math.Max(0, (int)Math.Floor((bar.Low - area.Minimum) / area.RowSize));
					endLevel = Math.Min(_volumes.Length - 1, (int)Math.Floor((bar.High - area.Minimum - Symbol.TickSize / 2) / area.RowSize));
				}

				var volumePerLevel = bar.Volume / (endLevel - startLevel + 1);

				if (bar.Close > bar.Open)
				{
					buyVolume = volumePerLevel;
				}
				else if (bar.Close < bar.Open)
				{
					sellVolume = volumePerLevel;
				}
				else
				{
					buyVolume = sellVolume = volumePerLevel / 2;
				}

				for (var level = startLevel; level <= endLevel; level++)
				{
					_volumes[level] ??= new();
					_volumes[level].Buy += buyVolume;
					_volumes[level].Sell += sellVolume;
				}
			}
		}

		_pocIndex = 0;

		for (var i = 0; i < _volumes.Length; i++)
		{
			if (_volumes[i] is null)
			{
				_volumes[i] = new();
			}

			if (_volumes[_pocIndex] < _volumes[i])
			{
				_pocIndex = i;
			}
		}

		var accumulatedVolume = _volumes[_pocIndex].Total;
		var totalVolume = _volumes.Sum(x => x.Total);
		var targetVolume = totalVolume * (ValueAreaPercent / 100);

		_vahIndex = _pocIndex;
		_valIndex = _pocIndex;

		while (accumulatedVolume < targetVolume)
		{
			var expanded = false;

			var valIndex = _valIndex;
			var vahIndex = _vahIndex;
			var volumeAbove = 0.0;
			var volumeBelow = 0.0;

			for (var i = 0; i < 2; i++)
			{
				if (valIndex > 0)
				{
					volumeBelow += _volumes[--valIndex].Total;
				}

				if (vahIndex < _volumes.Length - 1)
				{
					volumeAbove += _volumes[++vahIndex].Total;
				}
			}

			if (volumeAbove > volumeBelow)
			{
				accumulatedVolume += volumeAbove;

				if (_vahIndex < vahIndex)
				{
					_vahIndex = vahIndex;
					expanded = true;
				}
			}
			else
			{
				accumulatedVolume += volumeBelow;

				if (_valIndex > valIndex)
				{
					_valIndex = valIndex;
					expanded = true;
				}
			}

			if (expanded is false)
			{
				break;
			}
		}
	}

	private void CalculateVwap(Area area)
	{
		if (area.FromIndex == area.ToIndex || area.Bars is null)
		{
			_vwap = [];
			return;
		}

		var cumulativeVolume = 0.0;
		var cumulativeVolumePrice = 0.0;
		var vwap = new List<double>();

		for (var index = area.FromIndex; index <= area.ToIndex; index++)
		{
			var bar = Bars[index];
			var typicalPrice = (bar.High + bar.Low + bar.Close) / 3;

			cumulativeVolume += bar.Volume;
			cumulativeVolumePrice += typicalPrice * bar.Volume;

			vwap.Add(cumulativeVolumePrice / cumulativeVolume);
		}

		_vwap = [.. vwap];
	}

	private void Render(IDrawingContext context)
	{
		var area = _area;
		if (area.FromIndex > Chart.LastVisibleBarIndex || area.ToIndex < Chart.FirstVisibleBarIndex)
		{
			return;
		}

		var highY = ChartScale.GetYCoordinateByValue(area.High);
		var lowY = ChartScale.GetYCoordinateByValue(area.Low);
		var leftX = Chart.GetXCoordinateByBarIndex(area.FromIndex);
		var rightX = ExtendRight ? Chart.GetXCoordinateByBarIndex(area.ToIndex) : Math.Max(Points[0].X, Points[1].X);

		context.DrawRectangle(new Point(leftX, highY), new Point(rightX, lowY), null, BoxLineColor, BoxLineThickness, BoxLineStyle);

		var barsUsed = area.Bars is null ? "null" : $"{(area.Bars.ToIndex - area.Bars.FromIndex)}/{_bars.Count}";
		context.DrawText(new Point(leftX, lowY), $"Count: {area.Rows}, Size: {area.RowSize}, Bars: {barsUsed}, Volume: {area.Volume}, VAH: {_vahIndex}, VAL: {_valIndex}", BoxLineColor, Font);

		if (area.Rows == 0 || area.Bars is null)
		{
			return;
		}

		var offset = area.IsTickSize ? Symbol.TickSize / 2 : 0;
		var maximumY = ChartScale.GetYCoordinateByValue(area.Maximum - offset);
		var minimumY = ChartScale.GetYCoordinateByValue(area.Minimum - offset);

		var pixelsPerUnitY = Math.Abs(maximumY - minimumY) / (area.Maximum - area.Minimum);
		var adjustSpacing = area.RowSize * pixelsPerUnitY > 5;
		var lineThickness = adjustSpacing ? 2 : 1;
		var x = RowsPlacement is PlacementType.Left ? leftX : rightX;
		var boxWidth = RowsPlacement is PlacementType.Left ? rightX - leftX : leftX - rightX;

		for (var i = 0; i < area.Rows; i++)
		{
			var volume = _volumes[i];
			var y = minimumY - i * area.RowSize * pixelsPerUnitY - 1;
			if (y < 0)
			{
				break;
			}

			var volumeRatio = volume.Total / _volumes[_pocIndex].Total;
			var barWidth = boxWidth * (RowsWidthPercent / 100) * volumeRatio;
			var barHeight = area.RowSize * pixelsPerUnitY - (adjustSpacing ? 1 : 0);

			if (y - barHeight > Chart.Height)
			{
				continue;
			}

			if (PocLineVisible && i == _pocIndex)
			{
				var pointA = new Point(leftX, y - barHeight / 2);
				var pointB = new Point(rightX, pointA.Y);

				DrawPriceLevel(context, pointA, pointB, PocLineColor, PocLineThickness, PocLineStyle);
			}

			if (ValLineVisible && i == _valIndex)
			{
				var pointA = new Point(leftX, y);
				var pointB = new Point(rightX, pointA.Y);

				DrawPriceLevel(context, pointA, pointB, ValLineColor, ValLineThickness, ValLineStyle);
			}

			if (VahLineVisible && i == _vahIndex)
			{
				var pointA = new Point(leftX, y - barHeight);
				var pointB = new Point(rightX, pointA.Y);

				DrawPriceLevel(context, pointA, pointB, VahLineColor, VahLineThickness, VahLineStyle);
			}

			var color = i > _vahIndex ? ValueAreaAboveColor : i < _valIndex ? ValueAreaBelowColor : ValueAreaColor;

			DrawColumn(context, new(x, y), barWidth, barHeight, color, lineThickness);
		}

		if (VwapEnabled && _vwap.Length > 0)
		{
			var points = new Point[_vwap.Length];

			for (var i = 0; i < points.Length; i++)
			{
				points[i] = new Point
				{
					X = Chart.GetXCoordinateByBarIndex(area.FromIndex + i),
					Y = ChartScale.GetYCoordinateByValue(_vwap[i])
				};
			}

			context.DrawPolygon(points, null, VwapLineColor, VwapLineThickness, VwapLineStyle);
		}
	}

	private void DrawPriceLevel(IDrawingContext context, Point pointA, Point pointB, Color color, int thickness, LineStyle lineStyle)
	{
		context.DrawLine(pointA, pointB, color, thickness, lineStyle);

		if (ShowPrices is false)
		{
			return;
		}

		var price = Symbol.RoundToTick(ChartScale.GetValueByYCoordinate(pointA.Y));
		var text = Symbol.FormatPrice(price);
		var textOrigin = new Point(pointA);

		if (RowsPlacement is PlacementType.Left)
		{
			textOrigin.X = pointB.X - context.MeasureText(text, Font).Width;
		}

		context.DrawText(textOrigin, text, color, Font);
	}

	private static void DrawColumn(IDrawingContext context, Point point, double width, double height, Color color, int lineThickness)
	{
		context.DrawRectangle(point, new Point(point.X + width, point.Y - height), color, null, lineThickness);
	}
}
