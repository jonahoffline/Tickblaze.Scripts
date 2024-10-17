namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// CurrentDayOHL [CDOHL]
/// </summary>
public partial class CurrentDayOHL : Indicator
{
	[Parameter("Start time"), NumericRange(0, 2359, 1)]
	public int StartTime { get; set; } = 930;

	[Parameter("End time"), NumericRange(0, 2359, 1)]
	public int EndTime { get; set; } = 1600;

	[Plot("Open")]
	public PlotSeries POpen { get; set; } = new(Color.Orange, LineStyle.Dash);

	[Plot("High")]
	public PlotSeries PHigh { get; set; } = new(Color.Red, LineStyle.Dash);

	[Plot("Low")]
	public PlotSeries PLow { get; set; } = new(Color.Blue, LineStyle.Dash);

	private double _highestHigh = double.MinValue;
	private double _lowestLow = double.MaxValue;
	private double _open;
	private string _dayIdStart = "";
	private string _dayIdEnded = "";

	public CurrentDayOHL()
	{
		Name = "Current Day OHL";
		ShortName = "CDOHL";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var t0 = ToTime(Bars[index].Time) / 100;
		var day = Bars[index].Time.Day;
		var bartimeStr = Bars[index].Time.ToShortDateString();
		var isEndOfSession = false;

		//determine if EndTime was encountered today
		if (t0 >= EndTime && _dayIdEnded.CompareTo(bartimeStr) != 0)
		{
			_dayIdEnded = bartimeStr;
			isEndOfSession = true;
		}

		if (StartTime < EndTime)
		{
			//Start time encountered, set variables
			if (t0 > StartTime && _dayIdStart.CompareTo(bartimeStr) != 0)
			{
				_dayIdStart = bartimeStr;
				_open = Bars[index].Open;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
			}
			//while we're between start and end, find HH and LL
			else if (t0 > StartTime && t0 <= EndTime)
			{
				if (_highestHigh < Bars[index].High)
				{
					_highestHigh = Math.Max(_highestHigh, Bars[index].High);
				}

				_lowestLow = Math.Min(_lowestLow, Bars[index].Low);
			}
		}
		else if (StartTime > EndTime)
		{
			if (t0 > StartTime && _dayIdStart.CompareTo(bartimeStr) != 0)
			{
				_dayIdStart = bartimeStr;
				_open = Bars[index].Open;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
			}
			else if (t0 > StartTime || t0 <= EndTime)
			{
				_highestHigh = Math.Max(_highestHigh, Bars[index].High);
				_lowestLow = Math.Min(_lowestLow, Bars[index].Low);
			}
		}
		else if (StartTime == EndTime)
		{
			if (isEndOfSession)
			{
				_open = Bars[index].Open;
				_highestHigh = Bars[index].High;
				_lowestLow = Bars[index].Low;
			}
			else
			{
				_highestHigh = Math.Max(_highestHigh, Bars[index].High);
				_lowestLow = Math.Min(_lowestLow, Bars[index].Low);
			}
		}

		POpen[index] = _open;
		PHigh[index] = _highestHigh;
		PLow[index] = _lowestLow;
	}

	private static int ToTime(DateTime t)
	{
		return t.Hour * 10000 + t.Minute * 100 + t.Second;
	}
}