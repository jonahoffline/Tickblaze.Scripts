using System.ComponentModel;
using System.Diagnostics;

namespace Tickblaze.Scripts.Indicators;

//[Browsable(false)]
public class TestIndicator : Indicator
{
	[Plot("Bar Index")]
	public PlotSeries Result { get; set; } = new(Color.Transparent, PlotStyle.Histogram);

	private readonly Color[] _colors = [Color.Red, Color.Green, Color.Blue, Color.White];
	private int _index, _colorIndex;
	private int _lastIndex = -1;

	protected override void Calculate(int index)
	{
		Result[index] = index;
		Result.Colors[index] = _colors[_colorIndex];

		if (_lastIndex > index)
		{
			Debugger.Break();
		}

		_lastIndex = index;

		if (_index != index)
		{
			_index = index;

			//_colorIndex = _colorIndex % 3;

			if (++_colorIndex >= _colors.Length)
			{
				_colorIndex = 0;
			}
		}
	}
}
