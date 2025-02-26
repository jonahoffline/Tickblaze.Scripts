using System.Globalization;

namespace Tickblaze.Scripts.Indicators;

public partial class VolumeDelta : Indicator
{
	[Parameter("Cumulative")]
	public bool IsCumulative { get; set; } = false;

	[Parameter("Anchor period")]
	public AnchorPeriodType AnchorPeriod { get; set; } = AnchorPeriodType.Daily;

	[Parameter("Up", GroupName = "Colors")]
	public Color UpColor { get; set; } = "#008b8b";

	[Parameter("Down", GroupName = "Colors")]
	public Color DownColor { get; set; } = "#ff6347";

	[Plot("Close")]
	public PlotSeries Close { get; set; } = new("#00000000", PlotStyle.Dot, 1) { IsEditorBrowsable = false };

	public DataSeries Open { get; set; }
	public DataSeries High { get; set; }
	public DataSeries Low { get; set; }

	public enum AnchorPeriodType
	{
		Daily,
		Weekly,
		Monthly,
		Yearly
	}

	private record DataPoint(double Price, bool IsUp);

	private int _index;
	private BarSeries _bars;
	private DataPoint _lastDataPoint = new(0, false);
	private IExchangeSession _lastSession;

	public VolumeDelta()
	{
		Name = "Volume Delta";
		IsOverlay = false;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		if (IsCumulative is false)
		{
			parameters.Remove(nameof(AnchorPeriod));
		}

		return parameters;
	}

	protected override void Initialize()
	{
		Open = new();
		High = new();
		Low = new();

		var barSeriesInfo = new BarSeriesInfo
		{
			Period = Bars.Period.Source switch
			{
				BarPeriod.SourceType.Ask or
				BarPeriod.SourceType.Bid or
				BarPeriod.SourceType.Trade => new(Bars.Period.Source, BarPeriod.PeriodType.Tick, 1),
				BarPeriod.SourceType.Minute => new(BarPeriod.SourceType.Trade, Bars.Period.Size <= 15 ? BarPeriod.PeriodType.Second : BarPeriod.PeriodType.Minute, 1),
				BarPeriod.SourceType.Day => new(BarPeriod.SourceType.Minute, BarPeriod.PeriodType.Minute, 5),
				_ => throw new NotImplementedException(),
			}
		};

		_index = 0;
		_bars = GetBars(barSeriesInfo);

		//foreach (var plot in new PlotSeries[] { Open, High, Low })
		//{
		//	plot.PriceMarker.IsVisible = false;
		//}
	}

	protected override void Calculate(int index)
	{
		for (; _index < _bars.Count; _index++)
		{
			var bar = _bars[_index];
			var price = bar.Close;
			var volume = bar.Volume;
			var isUp = _lastDataPoint is null ? bar.Close > bar.Open : _lastDataPoint.Price < price || (_lastDataPoint.Price <= price && _lastDataPoint.IsUp);

			index = GetBarIndex(bar.Time);

			if (double.IsNaN(Close[index]))
			{
				if (IsCumulative)
				{
					var session = Symbol.ExchangeCalendar.GetSession(Bars[index].Time);
					var isNewAnchor = _lastSession is null;

					if (isNewAnchor is false && session != _lastSession)
					{
						var lastSessionStart = _lastSession.StartExchangeDateTime;
						var sessionStart = session!.StartExchangeDateTime;

						isNewAnchor = AnchorPeriod switch
						{
							AnchorPeriodType.Daily => true,
							AnchorPeriodType.Weekly => IsNewWeek(lastSessionStart, sessionStart),
							AnchorPeriodType.Monthly => lastSessionStart.Month != sessionStart.Month,
							AnchorPeriodType.Yearly => lastSessionStart.Year < sessionStart.Year,
							_ => throw new NotImplementedException()
						};
					}

					Open[index] = High[index] = Low[index] = Close[index] = !isNewAnchor && index > 0 ? Close[index - 1] : 0;

					_lastSession = session;
				}
				else
				{
					Open[index] = High[index] = Low[index] = Close[index] = 0;
				}
			}

			Close[index] += isUp ? volume : -volume;
			Close.Colors[index] = Close[index] > 0 ? UpColor : DownColor;
			Close.IsLineBreak[index] = true;

			High[index] = Math.Max(High[index], Close[index]);
			Low[index] = Math.Min(Low[index], Close[index]);

			_lastDataPoint = new(bar.Close, isUp);
		}
	}

	private int GetBarIndex(DateTime time)
	{
		for (var index = Bars.Count - 1; index >= 0; index--)
		{
			var barTime = Bars[index].Time;
			if (barTime <= time)
			{
				return index;
			}
		}

		return 0;
	}

	private static bool IsNewWeek(DateTime time1, DateTime time2)
	{
		var week1 = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time1, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
		var week2 = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time2, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

		return week1 != week2;
	}

	public override void OnRender(IDrawingContext context)
	{
		var bodyThickness = (int)Math.Round(Chart.DatapointWidth * 0.8);

		for (var index = Chart.FirstVisibleBarIndex; index <= Chart.LastVisibleBarIndex; index++)
		{
			var open = Open[index];
			var high = High[index];
			var low = Low[index];
			var close = Close[index];
			var color = close > open ? UpColor : DownColor;

			var x = Chart.GetXCoordinateByBarIndex(index);
			var openY = ChartScale.GetYCoordinateByValue(open);
			var highY = ChartScale.GetYCoordinateByValue(high);
			var lowY = ChartScale.GetYCoordinateByValue(low);
			var closeY = ChartScale.GetYCoordinateByValue(close);

			context.DrawLine(new Point(x, highY), new Point(x, lowY), color);

			if (bodyThickness > 1)
			{
				if (Math.Abs(openY - closeY) < 1)
				{
					openY = closeY + 1;
				}

				context.DrawLine(new Point(x, openY), new Point(x, closeY), color, bodyThickness);
			}
		}
	}

	public override (double Min, double Max) GetYRange()
	{
		var min = double.MaxValue;
		var max = double.MinValue;

		for (var index = Chart.FirstVisibleBarIndex; index <= Chart.LastVisibleBarIndex; index++)
		{
			max = Math.Max(max, High[index]);
			min = Math.Min(min, Low[index]);
		}

		return (min, max);
	}
}
