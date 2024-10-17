namespace Tickblaze.Scripts.Drawings;

public sealed class RiskReward : Drawing
{
	[Parameter("Stop Risk $")]
	public double StopRiskValue { get; set; } = 2000;

	[Parameter("Entry Color")]
	public Color EntryColor { get; set; } = Color.Gray;

	[Parameter("Stop Color")]
	public Color StopColor { get; set; } = Color.Red;

	[Parameter("T1 Enabled")]
	public bool TargetEnabled1 { get; set; } = true;

	[Parameter("T1 Ratio")]
	public double TargetRewardRatio1 { get; set; } = 1.5;

	[Parameter("T1 Exit %")]
	public double TargetPercent1 { get; set; } = 50;

	[Parameter("T1 Color")]
	public Color TargetColor1 { get; set; } = "#00ff00";

	[Parameter("T2 Enabled")]
	public bool TargetEnabled2 { get; set; } = true;

	[Parameter("T2 Ratio")]
	public double TargetRewardRatio2 { get; set; } = 2.5;

	[Parameter("T2 Exit %")]
	public double TargetPercent2 { get; set; } = 30;

	[Parameter("T2 Color")]
	public Color TargetColor2 { get; set; } = "#00ff7f";

	[Parameter("T3 Enabled")]
	public bool TargetEnabled3 { get; set; } = true;

	[Parameter("T3 Ratio")]
	public double TargetRewardRatio3 { get; set; } = 3.5;

	[Parameter("T3 Exit %")]
	public double TargetPercent3 { get; set; } = 20;

	[Parameter("T3 Color")]
	public Color TargetColor3 { get; set; } = "#00ced1";

	[Parameter("Text Font")]
	public Font TextFont { get; set; } = new("Arial", 12);

	[Parameter("Text Position")]
	public TextPositionType TextPosition { get; set; } = TextPositionType.Left;

	[Parameter("Lines thickness"), NumericRange(1, 5)]
	public int LineThickness { get; set; } = 1;

	[Parameter("Lines style")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Extend lines left")]
	public bool ExtendLinesLeft { get; set; }

	[Parameter("Extend lines right")]
	public bool ExtendLinesRight { get; set; }

	[Parameter("Show Price")]
	public bool ShowPrice { get; set; } = true;

	[Parameter("Show Ticks")]
	public bool ShowTicks { get; set; } = true;

	[Parameter("Show Quantity")]
	public bool ShowQuantity { get; set; } = true;

	[Parameter("Show PnL")]
	public bool ShowProfit { get; set; } = true;

	public IChartPoint PointA => Points[0];
	public IChartPoint PointB => Points[1];
	public int Direction => PointA.Y < PointB.Y ? 1 : -1;

	public override int PointsCount => 2;

	public enum TextPositionType
	{
		Left,
		Right,
	}

	private enum PriceLevelType
	{
		Entry,
		StopLoss,
		TakeProfit
	}

	private record Target(double Ratio, double ExitPercent, Color Color);

	public RiskReward()
	{
		Name = "Risk Reward";
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		Points[index].Value = Symbol.RoundToTick((double)yDataValue);
	}

	public override void OnRender(IDrawingContext context)
	{
		var entryPrice = (double)PointA.Value;
		var stopPrice = (double)PointB.Value;
		var stopTicks = (int)Math.Round(Math.Round(entryPrice - stopPrice, Symbol.Decimals) / Symbol.TickSize);
		var stopQuantity = Math.Max(Symbol.MinimumVolume, Symbol.NormalizeVolume(StopRiskValue * Direction / (stopTicks * Symbol.TickValue), RoundingMode.Down));
		var stopLoss = stopTicks * Symbol.TickValue * (double)stopQuantity;

		DrawPriceLevel(context, PriceLevelType.Entry, stopQuantity, entryPrice, 0, 0, EntryColor);
		DrawPriceLevel(context, PriceLevelType.StopLoss, stopQuantity, stopPrice, -stopTicks, -stopLoss, StopColor);

		var remainingQuantity = stopQuantity;
		var targets = new List<Target>();

		if (TargetEnabled1)
		{
			targets.Add(new(TargetRewardRatio1, TargetPercent1, TargetColor1));
		}

		if (TargetEnabled2)
		{
			targets.Add(new(TargetRewardRatio2, TargetPercent2, TargetColor2));
		}

		if (TargetEnabled3)
		{
			targets.Add(new(TargetRewardRatio3, TargetPercent3, TargetColor3));
		}

		for (var i = 0; i < targets.Count; i++)
		{
			var target = targets[i];
			var price = Symbol.RoundToTick(entryPrice + stopTicks * Symbol.TickSize * target.Ratio);
			var ticks = (int)Math.Round(Symbol.RoundToTick(price - entryPrice) / Symbol.TickSize);
			var quantity = Math.Max(0, Symbol.NormalizeVolume((double)stopQuantity * target.ExitPercent / 100.0, RoundingMode.Up));
			if (quantity > remainingQuantity || i == targets.Count - 1)
			{
				quantity = remainingQuantity;
			}

			var profit = (double)quantity * ticks * Symbol.TickValue;

			remainingQuantity -= quantity;

			DrawPriceLevel(context, PriceLevelType.TakeProfit, quantity, price, ticks, profit, target.Color);
		}
	}

	private void DrawPriceLevel(IDrawingContext context, PriceLevelType type, decimal quantity, double price, int ticks, double profit, Color color)
	{
		var y = ChartScale.GetYCoordinateByValue(price);
		var pointA = new Point(Math.Min(PointA.X, PointB.X), y);
		var pointB = new Point(Math.Max(PointA.X, PointB.X), y);

		ticks *= Direction;

		if (profit.Equals(0) is false)
		{
			profit *= Direction;
		}

		var textValues = new List<string>();
		var label = type switch
		{
			PriceLevelType.Entry => Direction > 0 ? "Long" : "Short",
			PriceLevelType.StopLoss => "Stop",
			PriceLevelType.TakeProfit => "Target",
			_ => throw new NotImplementedException(),
		};

		textValues.Add(label);

		if (ShowPrice)
		{
			textValues.Add(price.ToString($"F{Symbol.Decimals}"));
		}

		if (ShowQuantity)
		{
			textValues.Add($"Qty: {quantity}");
		}

		if (type is not PriceLevelType.Entry)
		{
			if (ShowTicks)
			{
				textValues.Add($"Ticks: {ticks}");
			}

			if (ShowProfit)
			{
				textValues.Add($"PnL: {profit:F2}");
			}
		}

		var text = string.Join(" | ", textValues);
		var textSize = context.MeasureText(text, TextFont);
		var textOrigin = new Point(TextPosition is TextPositionType.Left ? pointA.X : pointB.X - textSize.Width, y - textSize.Height);

		if (ExtendLinesRight)
		{
			pointB.X = context.RenderSize.Width;

			if (TextPosition is TextPositionType.Right)
			{
				textOrigin.X = pointB.X - textSize.Width - 2;
			}
		}

		if (ExtendLinesLeft)
		{
			pointA.X = 0;

			if (TextPosition is TextPositionType.Left)
			{
				textOrigin.X = 3;
			}
		}

		context.DrawLine(pointA, pointB, color, LineThickness, LineStyle);
		context.DrawText(textOrigin, text, color, TextFont);
	}
}
