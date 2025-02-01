using System.Globalization;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Pivot Points
/// </summary>
public partial class PivotPoints : Indicator
{
	internal const string LabelsGroupName = "Labels";

	[Parameter("Period")]
	public PeriodType Period { get; set; } = PeriodType.Daily;

	[Parameter("Type")]
	public CalculationType Type { get; set; } = CalculationType.Floor;

	[Parameter("Display", GroupName = LabelsGroupName)]
	public LabelDisplayType LabelsDisplay { get; set; } = LabelDisplayType.Hidden;

	[Parameter("Font", GroupName = LabelsGroupName)]
	public Font LabelsFont { get; set; } = new("Arial", 12);

	[Plot("PP")]
	public PlotSeries PP { get; set; } = new(Color.Blue);

	[Plot("R1")]
	public PlotSeries R1 { get; set; } = new(Color.Red);

	[Plot("R2")]
	public PlotSeries R2 { get; set; } = new(Color.Red);

	[Plot("R3")]
	public PlotSeries R3 { get; set; } = new(Color.Red);

	[Plot("R4")]
	public PlotSeries R4 { get; set; } = new(Color.Red);

	[Plot("S1")]
	public PlotSeries S1 { get; set; } = new(Color.Green);

	[Plot("S2")]
	public PlotSeries S2 { get; set; } = new(Color.Green);

	[Plot("S3")]
	public PlotSeries S3 { get; set; } = new(Color.Green);

	[Plot("S4")]
	public PlotSeries S4 { get; set; } = new(Color.Green);

	public enum PeriodType
	{
		Daily,
		Weekly,
		Monthly,
		Yearly
	}

	public enum CalculationType
	{
		Floor,
		Woodie,
		Camarilla,
		[DisplayName("Tom DeMark's")] TomDeMark,
		Fibonacci
	}

	public enum LabelDisplayType
	{
		[DisplayName("Hidden")] Hidden,
		[DisplayName("Name")] Name,
		[DisplayName("Price")] Price,
		[DisplayName("Name with price")] NameAndPrice
	}

	private IExchangeSession _lastSession;
	private double _open, _high, _low, _close;

	public PivotPoints()
	{
		Name = "Pivot Points";
		ShortName = "PP";
		IsOverlay = true;
		AutoRescale = false;
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		var time = bar.Time;

		var session = Bars.Symbol.ExchangeCalendar.GetSession(time);
		var isFirstSession = _lastSession is null;
		var isNewSession = isFirstSession;

		if (isFirstSession is false && session != _lastSession)
		{
			var lastSessionStart = _lastSession.StartExchangeDateTime;
			var sessionStart = session!.StartExchangeDateTime;

			isNewSession = Period switch
			{
				PeriodType.Daily => true,
				PeriodType.Weekly => IsNewWeek(lastSessionStart, sessionStart),
				PeriodType.Monthly => lastSessionStart.Month != sessionStart.Month,
				PeriodType.Yearly => lastSessionStart.Year < sessionStart.Year,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		if (isNewSession)
		{
			if (isFirstSession is false)
			{
				foreach (var plot in Plots)
				{
					plot.IsLineBreak[index] = true;
				}

				var result = Calculate(_open, _high, _low, _close, Type);

				PP[index] = result.PP;
				R1[index] = result.R1;
				R2[index] = result.R2;
				R3[index] = result.R3;
				R4[index] = result.R4;
				S1[index] = result.S1;
				S2[index] = result.S2;
				S3[index] = result.S3;
				S4[index] = result.S4;
			}

			_open = bar.Open;
			_high = bar.High;
			_low = bar.Low;
			_close = bar.Close;
		}
		else if (index > 0)
		{
			_high = Math.Max(_high, bar.High);
			_low = Math.Min(_low, bar.Low);
			_close = bar.Close;

			foreach (var plot in Plots)
			{
				plot[index] = plot[index - 1];
			}
		}

		_lastSession = session;
	}

	public override void OnRender(IDrawingContext context)
	{
		if (LabelsDisplay is LabelDisplayType.Hidden)
		{
			return;
		}

		foreach (var plot in Plots)
		{
			if (plot.IsVisible is false)
			{
				continue;
			}

			var showLabel = true;

			for (var index = Chart.LastVisibleBarIndex; index >= Chart.FirstVisibleBarIndex; index--)
			{
				var value = plot[index];

				if (showLabel && double.IsNaN(value) is false)
				{
					var text = LabelsDisplay switch
					{
						LabelDisplayType.Hidden => string.Empty,
						LabelDisplayType.Name => plot.Name,
						LabelDisplayType.Price => Symbol.FormatPrice(value),
						LabelDisplayType.NameAndPrice => $"{plot.Name} @ {Symbol.FormatPrice(value)}",
						_ => throw new NotImplementedException(),
					};

					var textSize = context.MeasureText(text, LabelsFont);
					var origin = new Point()
					{
						X = Chart.GetXCoordinateByBarIndex(index) - textSize.Width,
						Y = ChartScale.GetYCoordinateByValue(value) - textSize.Height
					};

					context.DrawText(origin, text, plot.Color, LabelsFont);
					showLabel = false;
				}

				if (plot.IsLineBreak[index])
				{
					showLabel = true;
				}
			}
		}
	}

	private static bool IsNewWeek(DateTime time1, DateTime time2)
	{
		var week1 = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time1, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
		var week2 = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time2, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

		return week1 != week2;
	}

	private static (double PP, double R1, double R2, double R3, double R4, double S1, double S2, double S3, double S4) Calculate(double open, double high, double low, double close, CalculationType type)
	{
		double pp, r1, r2, r3, r4, s1, s2, s3, s4;
		pp = r1 = r2 = r3 = r4 = s1 = s2 = s3 = s4 = double.NaN;

		switch (type)
		{
		case CalculationType.Floor:
		{
			pp = (high + low + close) / 3;
			r1 = 2 * pp - low;
			r2 = pp + (high - low);
			r3 = high + 2 * (pp - low);
			s1 = 2 * pp - high;
			s2 = pp - (high - low);
			s3 = low - 2 * (high - pp);
			break;
		}
		case CalculationType.Woodie:
		{
			pp = (high + low + 2 * close) / 4;
			r1 = 2 * pp - low;
			r2 = pp + (high - low);
			r3 = high + 2 * (pp - low);
			s1 = 2 * pp - high;
			s2 = pp - (high - low);
			s3 = low - 2 * (high - pp);
			break;
		}
		case CalculationType.Camarilla:
		{
			pp = close;
			r1 = close + (high - low) * 1.1 / 12;
			r2 = close + (high - low) * 1.1 / 6;
			r3 = close + (high - low) * 1.1 / 4;
			r4 = close + (high - low) * 1.1 / 2;
			s1 = close - (high - low) * 1.1 / 12;
			s2 = close - (high - low) * 1.1 / 6;
			s3 = close - (high - low) * 1.1 / 4;
			s4 = close - (high - low) * 1.1 / 2;
			break;
		}
		case CalculationType.TomDeMark:
		{
			var x = close > open ? 2 * high + low + close : close < open ? 2 * low + high + close : 2 * close + high + low;
			pp = x / 4;
			r1 = x / 2 - low;
			r2 = high - low + x / 4;
			r3 = high - low * 2 + x / 2;
			s1 = x / 2 - high;
			s2 = low - high + x / 4;
			s3 = low - high * 2 + x / 2;
			break;
		}
		case CalculationType.Fibonacci:
		{
			pp = (high + low + close) / 3;
			r1 = pp + ((high - low) * 0.382);
			r2 = pp + ((high - low) * 0.618);
			r3 = pp + ((high - low) * 1.000);
			s1 = pp - ((high - low) * 0.382);
			s2 = pp - ((high - low) * 0.618);
			s3 = pp - ((high - low) * 1.000);
			break;
		}
		default:
			break;
		}

		return (pp, r1, r2, r3, r4, s1, s2, s3, s4);
	}
}