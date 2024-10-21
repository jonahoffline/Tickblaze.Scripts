
namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// ARC CriticalAverages [CrAv]
/// </summary>
public partial class CriticalAverages : Indicator
{
	[Parameter("Avgeraging Period"), NumericRange(1, int.MaxValue)]
	public int AveragingPeriod { get; set; } = 14;

	[Parameter("Start Time")]
	public int StartTimeOfDay { get; set; } = 1700;

	[Plot("ProjectedHigh")]
	public PlotSeries ProjectedHigh { get; set; } = new("#00ffff", LineStyle.Solid);

	[Plot("ProjectedLow")]
	public PlotSeries ProjectedLow { get; set; } = new("#00ffff", LineStyle.Solid);

	[Plot("ProjectedClose1")]
	public PlotSeries ProjectedClose1 { get; set; } = new("#ffff00", LineStyle.Solid);

	[Plot("ProjectedClose2")]
	public PlotSeries ProjectedClose2 { get; set; } = new("#ffff00", LineStyle.Solid);

	private double _openCloseAverage = double.MinValue;
	private double _highLowAverage = double.MinValue;
	private SortedDictionary<DayOfWeek, List<double>> _openCloseRange = [];
	private SortedDictionary<DayOfWeek, List<double>> _highLowRange = [];
	private DayOfWeek _dayOfWeek = DayOfWeek.Saturday;
	private double _currentHigh = double.MinValue;
	private double _currentLow = double.MaxValue;
	private double _sessionOpen;
	private double _theOpen;
	private double _dailyHigh = double.MinValue;
	private double _dailyLow = double.MaxValue;
	private int _dayId = 0;
	private bool _isNewSession;
	private int _priorIndex = -1;

	public CriticalAverages()
	{
		Name = "ARC CriticalAverages";
		ShortName = "CrAv";
		IsOverlay = true;
	}

	private string _path = @"C:\Users\sbgtr\Documents\CriticalAverages_Tickblaze.txt";
	private string _pathc = @"C:\Users\sbgtr\Documents\CriticalAverages_TickblazeCloses.txt";
	private bool _isNewBar = false;
	private List<string> _tcdata = new();
	private List<string> _odata = new();

	protected override void Calculate(int index)
	{
		_isNewBar = index != _priorIndex;
		_tcdata.Add($"{Bars[index].Time.ToLocalTime()}   {Bars[index].Close}      index: {index}  priorIndex: {_priorIndex}");

		ProjectedClose1[index] = 0;
		ProjectedClose2[index] = 0;
		ProjectedHigh[index] = 0;
		ProjectedLow[index] = 0;

		if (index < 2)
		{
			return;
		}

		var timePriorBar = ToTime(Bars[index - 1].Time.ToLocalTime()) / 100;
		var timeCurrentBar = ToTime(Bars[index].Time.ToLocalTime()) / 100;
		_isNewSession = timePriorBar < StartTimeOfDay && timeCurrentBar >= StartTimeOfDay || (timePriorBar < StartTimeOfDay && timeCurrentBar < timePriorBar);

		if (_dayId == 0 || _isNewSession)
		{
			if (_sessionOpen == 0)
			{
				_sessionOpen = Bars[index].Open;
			}

			if (_currentLow != double.MaxValue)
			{
				_dayOfWeek = Bars[index - 1].Time.ToLocalTime().DayOfWeek;
				if (!_openCloseRange.TryGetValue(_dayOfWeek, out _))
				{
					_openCloseRange[_dayOfWeek] = [];
				}

				_openCloseRange[_dayOfWeek].Add(Math.Abs(_sessionOpen - Bars[index].Close));

				while (_openCloseRange[_dayOfWeek].Count > AveragingPeriod)
				{
					_openCloseRange[_dayOfWeek].RemoveAt(0);
				}

				if (!_highLowRange.TryGetValue(_dayOfWeek, out _))
				{
					_highLowRange[_dayOfWeek] = [];
				}

				_highLowRange[_dayOfWeek].Add(Math.Abs(_dailyHigh - _dailyLow));

				while (_highLowRange[_dayOfWeek].Count > AveragingPeriod)
				{
					_highLowRange[_dayOfWeek].RemoveAt(0);
				}

				if (_isNewBar)
				{
					_odata.Add($"{Bars[index].Time.ToLocalTime()}, OCRange{_openCloseRange[_dayOfWeek].Count}:, {_openCloseRange[_dayOfWeek][^1]}, HLRange{_highLowRange[_dayOfWeek].Count}:, {_highLowRange[_dayOfWeek][^1]}, H {Bars[index].High}, L {Bars[index].Low}\n");
				}
			}

			_dayId = Bars[index].Time.ToLocalTime().Day;
			_currentHigh = Bars[index].High;
			_currentLow = Bars[index].Low;
			_sessionOpen = Bars[index].Open;
		}

		_currentHigh = Math.Max(_currentHigh, Bars[index].High);
		_currentLow = Math.Min(_currentLow, Bars[index].Low);
		if (_priorIndex != index)//is first tick of bar
		{
			if (_openCloseRange.TryGetValue(_dayOfWeek, out _))
			{
				_openCloseAverage = _openCloseRange[_dayOfWeek].Average();
			}

			if (_highLowRange.TryGetValue(_dayOfWeek, out _))
			{
				_highLowAverage = _highLowRange[_dayOfWeek].Average();
			}
		}

		if (index < 2)
		{
			return;
		}

		if (_isNewSession || _dailyHigh == double.MinValue)
		{
			_dailyHigh = Bars[index].High;
			_dailyLow = Bars[index].Low;
			_theOpen = Bars[index].Open;
		}
		else
		{
			_dailyHigh = Math.Max(_dailyHigh, Bars[index].High);
			_dailyLow = Math.Min(_dailyLow, Bars[index].Low);
		}

		if (_openCloseAverage != double.MinValue && _dailyHigh != double.MinValue)
		{
			try
			{
				ProjectedClose1[index] = _theOpen + _openCloseAverage;
				ProjectedClose2[index] = _theOpen - _openCloseAverage;
			}
			catch { }
		}

		if (_highLowAverage != double.MinValue && _dailyHigh != double.MinValue)
		{
			try
			{
				var mid = (_dailyHigh + _dailyLow) / 2.0;

				var temp = mid + _highLowAverage / 2.0;
				ProjectedHigh[index] = temp == 0 ? ProjectedHigh[index - 1] : temp;

				temp = mid - _highLowAverage / 2.0;
				ProjectedLow[index] = temp == 0 ? ProjectedLow[index - 1] : temp;
			}
			catch { }
		}

		if (_isNewBar)
		{
			_odata.Add($"{Bars[index].Time.ToLocalTime()}, ProjectedHigh{ProjectedHigh[index]}:, ProjectedLow{ProjectedLow[index]}, ProjectedClose1{ProjectedClose1[index]}:, ProjectedClose2{ProjectedClose2[index]}\n");
		}

		if (false && Bars[index].Time.ToLocalTime().Month == 10 && Bars[index].Time.ToLocalTime().Day == 21 && _path.Length > 0)
		{
			System.IO.File.WriteAllLines(_path, _odata);
			System.IO.File.WriteAllLines(_pathc, _tcdata);
			_odata.Clear();
			_path = string.Empty;
		}

		_priorIndex = index;
	}

	private int ToTime(DateTime t)
	{
		var hr = t.Hour * 10000;
		var min = t.Minute * 100;
		var sec = t.Second;
		return hr + min + sec;
	}
}
