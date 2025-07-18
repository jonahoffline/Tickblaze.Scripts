namespace Tickblaze.Scripts.Indicators;

public partial class DailyLevels : Indicator
{
	internal const string OvernightGroupName = "Overnight";
	internal const string OpenRangeGroupName = "Open Range";
	internal const string LabelsGroupName = "Labels";

	internal static readonly Color OvernightColor = "#f1e78f";
	internal static readonly Color RegularHoursColor = Color.Purple;
	internal static readonly Color OpenRangeColor = "#2278d1";

	[Parameter("Enabled", GroupName = OvernightGroupName)]
	public bool OvernightEnabled { get; set; } = true;

	[Parameter("Time Type")]
	public TimeType SelectedTimeType { get; set; } = TimeType.Exchange;

	[Parameter("Start Time", GroupName = OvernightGroupName), NumericRange(0, 2359)]
	public int OvernightStartTime { get; set; } = 1700;

	[Parameter("End Time", GroupName = OvernightGroupName), NumericRange(0, 2359)]
	public int OvernightEndTime { get; set; } = 830;

	[Parameter("Enabled", GroupName = OpenRangeGroupName)]
	public bool OpenRangeEnabled { get; set; } = true;

	[Parameter("Start Time", GroupName = OpenRangeGroupName), NumericRange(0, 2359)]
	public int OpenRangeStartTime { get; set; } = 930;

	[Parameter("End Time", GroupName = OpenRangeGroupName), NumericRange(0, 2359)]
	public int OpenRangeEndTime { get; set; } = 1030;

	[Parameter("Show", GroupName = LabelsGroupName)]
	public bool LabelsEnabled { get; set; } = true;

	[Parameter("Display price", GroupName = LabelsGroupName)]
	public bool LabelsDisplayPrice { get; set; } = false;

	[Parameter("Font", GroupName = LabelsGroupName)]
	public Font LabelsFont { get; set; } = new("Arial", 12);

	[Parameter("Position", GroupName = LabelsGroupName)]
	public LabelPosition LabelsPosition { get; set; } = LabelPosition.Right;

	[Plot("24-Hr Mid")]
	public PlotSeries EthMid { get; set; } = new("#00ffff");

	[Plot("RTH Mid")]
	public PlotSeries RthMid { get; set; } = new("#ff00ff");

	[Plot("ON High")]
	public PlotSeries OvernightHigh { get; set; } = new(OvernightColor);

	[Plot("ON Low")]
	public PlotSeries OvernightLow { get; set; } = new(OvernightColor);

	[Plot("ON Mid")]
	public PlotSeries OvernightMid { get; set; } = new(OvernightColor);

	[Plot("ON High x 1.5")]
	public PlotSeries OvernightHigh15 { get; set; } = new(OvernightColor);

	[Plot("ON High x 2")]
	public PlotSeries OvernightHigh20 { get; set; } = new(OvernightColor);

	[Plot("ON Low x 1.5")]
	public PlotSeries OvernightLow15 { get; set; } = new(OvernightColor);

	[Plot("ON Low x 2")]
	public PlotSeries OvernightLow20 { get; set; } = new(OvernightColor);

	[Plot("OR High")]
	public PlotSeries OpenRangeHigh { get; set; } = new(OpenRangeColor);

	[Plot("OR Low")]
	public PlotSeries OpenRangeLow { get; set; } = new(OpenRangeColor);

	[Plot("OR Mid")]
	public PlotSeries OpenRangeMid { get; set; } = new(OpenRangeColor);

	[Plot("OR High x 1.5")]
	public PlotSeries OpenRangeHigh15 { get; set; } = new(OpenRangeColor);

	[Plot("OR High x 2")]
	public PlotSeries OpenRangeHigh20 { get; set; } = new(OpenRangeColor);

	[Plot("OR Low x 1.5")]
	public PlotSeries OpenRangeLow15 { get; set; } = new(OpenRangeColor);

	[Plot("OR Low x 2")]
	public PlotSeries OpenRangeLow20 { get; set; } = new(OpenRangeColor);

	public class Sessions : List<Session>
	{
		public TimeOnly StartTime { get; }
		public TimeOnly EndTime { get; }
		public Dictionary<PlotSeries, Func<Session, double>> Levels { get; init; }

		public Sessions(int startTime, int endTime)
		{
			StartTime = new(startTime / 100, startTime % 100);
			EndTime = new(endTime / 100, endTime % 100);
		}
	}

	public class Session(DateTime start, DateTime end)
	{
		public DateTime Start { get; } = start;
		public DateTime End { get; } = end;
		public double Open { get; private set; } = double.NaN;
		public double High { get; private set; } = double.NaN;
		public double Low { get; private set; } = double.NaN;
		public double Close { get; private set; } = double.NaN;
		public double Range => High - Low;
		public double Mid => (High + Low) / 2;
		public bool HasStarted { get; private set; }
		public bool HasFinished { get; private set; }

		public bool IsOpen(DateTime time)
		{
			return time >= Start && time < End;
		}

		public void Update(Bar bar)
		{
			if (HasFinished)
				return;

			if (IsOpen(bar.Time))
				if (HasStarted is false)
				{
					HasStarted = true;
					Open = bar.Open;
					High = bar.High;
					Low = bar.Low;
					Close = bar.Close;
				}
				else
				{
					High = Math.Max(High, bar.High);
					Low = Math.Min(Low, bar.Low);
					Close = bar.Close;
				}
			else if (HasStarted && bar.Time > End)
				HasFinished = true;
		}
	}

	public enum LabelPosition
	{
		Left,
		Right,
		Center
	}

	public enum TimeType
	{
		Local,
		Exchange
	}

	private bool _isDailyChart;
	private IExchangeSession _exchangeSession;
	private Sessions[] _sessions;
	private BarSeries _ethBars, _rthBars;
	private double? _ethHigh, _ethLow;
	private double? _rthHigh, _rthLow;

	public DailyLevels()
	{
		Name = "Daily Levels";
		IsOverlay = true;
	}

	protected override Parameters GetParameters(Parameters parameters)
	{
		var groups = new Dictionary<string, bool>
		{
			{ OvernightGroupName, OvernightEnabled },
			{ OpenRangeGroupName, OpenRangeEnabled },
			{ LabelsGroupName, LabelsEnabled }
		};

		foreach (var group in groups)
		{
			if (group.Value)
				continue;

			foreach (var key in parameters.Keys)
			{
				if (key.EndsWith("Enabled"))
					continue;

				if (parameters[key].Attributes.GroupName == group.Key)
					parameters.Remove(key);
			}
		}

		return parameters;
	}

	protected override void Initialize()
	{
		_isDailyChart = Bars.Period.Source is BarPeriod.SourceType.Day;

		if (_isDailyChart)
			return;

		var sessions = new List<Sessions>();

		if (OvernightEnabled)
			sessions.Add(new(OvernightStartTime, OvernightEndTime)
			{
				Levels = new()
				{
					{ OvernightHigh, (session) => session.High },
					{ OvernightLow, (session) => session.Low },
					{ OvernightMid, (session) => session.Mid },
					{ OvernightHigh15, (session) => session.High + session.Range / 2 },
					{ OvernightHigh20, (session) => session.High + session.Range },
					{ OvernightLow15, (session) => session.Low - session.Range / 2 },
					{ OvernightLow20, (session) => session.Low - session.Range },
				}
			});

		if (OpenRangeEnabled)
			sessions.Add(new(OpenRangeStartTime, OpenRangeEndTime)
			{
				Levels = new()
				{
					{ OpenRangeHigh, (session) => session.High },
					{ OpenRangeLow, (session) => session.Low },
					{ OpenRangeMid, (session) => session.Mid },
					{ OpenRangeHigh15, (session) => session.High + session.Range / 2 },
					{ OpenRangeHigh20, (session) => session.High + session.Range },
					{ OpenRangeLow15, (session) => session.Low - session.Range / 2 },
					{ OpenRangeLow20, (session) => session.Low - session.Range },
				}
			});

		_sessions = [.. sessions];

		var barSeriesInfo = new BarSeriesInfo { Period = Bars.Period, BarType = Bars.BarType };

		_ethBars = GetBars(barSeriesInfo with { IsETH = true });
		_rthBars = GetBars(barSeriesInfo with { IsETH = false });
	}

	protected override void Calculate(int index)
	{
		if (_isDailyChart)
			return;

		var bar = Bars[index];
		var exchangeSession = Symbol.ExchangeCalendar.GetSession(bar.Time);
		if (exchangeSession is not null && exchangeSession != _exchangeSession)
		{
			foreach (var sessionInfo in _sessions)
				if (SelectedTimeType == TimeType.Exchange)
				{
					var exchangeTime = exchangeSession.StartExchangeDateTime;
					var exchangeStartTime = GetNearestTime(exchangeTime, sessionInfo.StartTime);
					var exchangeEndTime = GetNearestTime(exchangeStartTime, sessionInfo.EndTime);

					var start = Symbol.ExchangeCalendar.ExchangeDateTimeToUtcDateTime(exchangeStartTime);
					var end = Symbol.ExchangeCalendar.ExchangeDateTimeToUtcDateTime(exchangeEndTime);

					sessionInfo.Add(new(start, end));
				}
				else
				{
					var localSessionStartTime = exchangeSession.StartUtcDateTime.ToLocalTime();
					var localStartTime = GetNearestTime(localSessionStartTime, sessionInfo.StartTime);
					var localEndTime = GetNearestTime(localStartTime, sessionInfo.EndTime);

					var start = localStartTime.ToUniversalTime();
					var end = localEndTime.ToUniversalTime();

					sessionInfo.Add(new(start, end));
				}

			_exchangeSession = exchangeSession;

			_ethHigh = null;
			_ethLow = null;
			_rthHigh = null;
			_rthLow = null;

			EthMid.IsLineBreak[index] = true;
			RthMid.IsLineBreak[index] = true;
		}
		else
		{
			if (RthMid.IsVisible)
			{
				var isOpen = _rthBars.Symbol.ExchangeCalendar.IsSessionOpen(bar.Time, true);
				if (isOpen)
				{
					_rthHigh = _rthHigh.HasValue ? Math.Max(_rthHigh.Value, bar.High) : bar.High;
					_rthLow = _rthLow.HasValue ? Math.Min(_rthLow.Value, bar.Low) : bar.Low;

					RthMid[index] = (_rthHigh.Value + _rthLow.Value) / 2;
				}
				else
					RthMid[index] = double.NaN;
			}

			if (EthMid.IsVisible)
			{
				var isOpen = _ethBars.Symbol.ExchangeCalendar.IsSessionOpen(bar.Time, true);
				if (isOpen)
				{
					_ethHigh = _ethHigh.HasValue ? Math.Max(_ethHigh.Value, bar.High) : bar.High;
					_ethLow = _ethLow.HasValue ? Math.Min(_ethLow.Value, bar.Low) : bar.Low;

					EthMid[index] = (_ethHigh.Value + _ethLow.Value) / 2;
				}
				else
					EthMid[index] = double.NaN;
			}
		}

		foreach (var sessions in _sessions)
			foreach (var session in sessions)
				session.Update(bar);

		foreach (var sessions in _sessions)
		{
			var lastSession = sessions.LastOrDefault();
			if (lastSession is null)
			{
				continue;
			}

			foreach (var (plotSeries, value) in sessions.Levels)
			{
				if (lastSession.HasFinished)
				{
					plotSeries[index] = value(lastSession);
				}
			}
		}
	}

	private static DateTime GetNearestTime(DateTime fromTime, TimeOnly timeOfDay)
	{
		var targetTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, timeOfDay.Hour, timeOfDay.Minute, 0);

		if (targetTime < fromTime)
			targetTime = targetTime.AddDays(1);

		return targetTime;
	}

	public override void OnRender(IDrawingContext context)
	{
		if (_isDailyChart || Bars.Count == 0)
			return;

		var firstBar = Bars[Chart.FirstVisibleBarIndex];
		var lastBar = Bars[Math.Min(Bars.Count - 1, Chart.LastVisibleBarIndex)];
		var fromTime = firstBar.Time;
		var lastTime = lastBar.Time;

		if (LabelsEnabled)
		{
			var plots = new[] { EthMid, RthMid };

			foreach (var plot in plots)
			{
				if (plot.IsVisible is false)
					continue;

				var showLabel = true;

				for (var index = Chart.LastVisibleBarIndex; index >= Chart.FirstVisibleBarIndex; index--)
				{
					var value = plot[index];

					if (showLabel && double.IsNaN(value) is false)
					{
						var text = LabelsDisplayPrice ? $"{plot.Name} @ {Symbol.FormatPrice(value)}" : plot.Name;
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
						showLabel = true;
				}
			}
		}

		var lastBarTime = Bars[^1].Time;
		var lastBarX = Chart.GetXCoordinateByBarIndex(Bars.Count - 1);

		foreach (var sessions in _sessions)
			foreach (var session in sessions)
			{
				if (session.HasStarted is false)
					continue;

				if (session.HasFinished is false)
					continue;

				var exchangeSession = Bars.Symbol.ExchangeCalendar.GetSession(session.Start);
				if (exchangeSession is null)
					continue;

				var timeToExtend = exchangeSession.EndUtcDateTime.AddDays(0);
				if (timeToExtend < fromTime)
					continue;

				var x1 = Chart.GetXCoordinateByTime(session.End);
				var x2 = Chart.GetXCoordinateByTime(timeToExtend);

				if (x1 > Chart.Width || x2 < 0)
					continue;

				if (x1 < 0)
					x1 = 0;

				if (x2 > Chart.Width)
					x2 = Chart.Width;

				foreach (var level in sessions.Levels)
				{
					var plot = level.Key;
					if (plot.IsVisible is false)
						continue;

					var value = level.Value(session);
					var y = ChartScale.GetYCoordinateByValue(value);
					var pointA = new Point(x1, y);
					var pointB = new Point(x2, y);

					var extendToFuture = timeToExtend > lastBarTime;
					if (extendToFuture)
					{
						context.DrawLine(new Point(lastBarX, y), pointB, plot.Color, plot.Thickness, plot.LineStyle);
					}

					if (LabelsEnabled)
					{
						var text = LabelsDisplayPrice ? $"{plot.Name} @ {Symbol.FormatPrice(value)}" : plot.Name;
						var textSize = context.MeasureText(text, LabelsFont);
						var originX = LabelsPosition switch
						{
							LabelPosition.Left => pointA.X,
							LabelPosition.Right => pointB.X - textSize.Width,
							LabelPosition.Center => (pointA.X + pointB.X) / 2 - textSize.Width / 2,
							_ => throw new NotImplementedException(),
						};

						var origin = new Point(originX, pointA.Y - textSize.Height);

						context.DrawText(origin, text, plot.Color, LabelsFont);
					}
				}
			}
	}
}