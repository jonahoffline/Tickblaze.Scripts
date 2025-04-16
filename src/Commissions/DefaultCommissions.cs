namespace Tickblaze.Scripts.Commissions;

public class DefaultCommissions : CommissionSimulator
{
	[Parameter("Min $ Per-Order", GroupName = "Stocks", Description = "Minimum commission amount per-order")]
	[NumericRange(0, double.MaxValue)]
	public double StockMinCommissionsPerOrder { get; set; } = 0;

	[Parameter("Max $ Per-Order", GroupName = "Stocks", Description = "Maximum commission charge per-order")]
	[NumericRange(0, double.MaxValue)]
	public double StockMaxCommissionsPerOrder { get; set; } = 100_000_000;

	[Parameter("$ Per-Side, Per-share", GroupName = "Stocks", Description = "Commission dollar amount per-order, per-share traded")]
	[NumericRange(0, double.MaxValue)]
	public double StockPerSidePerUnitCommission { get; set; } = 1;

	[Parameter("Max % of Order", GroupName = "Stocks", Description = "Maximum commission amount as a percentage of the total order")]
	[NumericRange(0, double.MaxValue)]
	public double StockMaxCommissionsPerOrderAsPercent { get; set; } = 1000;

	[Parameter("$ Per-Side, Per-Contract", GroupName = "Futures", Description = "Commission amount per-order, per-contract")]
	[NumericRange(0, double.MaxValue)]
	public double FuturesPerSidePerContractCommission { get; set; } = 2;

	[Parameter("Min $ Per-Order", GroupName = "Forex", Description = "Minimum commission amount per-order")]
	[NumericRange(0, double.MaxValue)]
	public double ForexMinCommissionsPerOrder { get; set; } = 0;

	[Parameter("Max $ Per-Order", GroupName = "Forex", Description = "Maximum commission charge per-order")]
	[NumericRange(0, double.MaxValue)]
	public double ForexMaxCommissionsPerOrder { get; set; } = 100_000_000;

	[Parameter("$ Per-Side, Per-Lot", GroupName = "Forex", Description = "Commission dollar amount per-order, per-lot traded")]
	[NumericRange(0, double.MaxValue)]
	public double ForexPerSidePerUnitCommission { get; set; } = 0;

	[Parameter("Max % of Order", GroupName = "Forex", Description = "Maximum commission amount as a percentage of the total order")]
	[NumericRange(0, double.MaxValue)]
	public double ForexMaxCommissionsPerOrderAsPercent { get; set; } = 1000;

	[Parameter("Minimum $ Commission", GroupName = "Crypto", Description = "Minimum commission amount per-order")]
	[NumericRange(0, double.MaxValue)]
	public double MinCryptoCommission { get; set; } = 0;

	[Parameter("% of Order", GroupName = "Crypto", Description = "Commission amount as a percentage of the total order")]
	[NumericRange(0, double.MaxValue)]
	public double PercentCryptoCommission { get; set; } = 0.15;

	public DefaultCommissions()
	{
		Name = "Per Order/Quantity";
		Description = "Adds simulated per order and per unit traded commissions to orders";
	}

	public override double Calculate(Symbol symbolInfo, double fillQuantity, double fillPrice, bool isFirstFill)
	{
		return symbolInfo.Type switch
		{
			InstrumentType.ETF or InstrumentType.Stock or InstrumentType.Index => Math.Max(StockMinCommissionsPerOrder, new[]
			{
				StockMaxCommissionsPerOrder, 
				fillQuantity * fillPrice * StockMaxCommissionsPerOrderAsPercent / 100, 
				StockPerSidePerUnitCommission * fillQuantity
			}.Min()),
			InstrumentType.Forex => Math.Max(ForexMinCommissionsPerOrder, new[]
			{
				ForexMaxCommissionsPerOrder, 
				fillQuantity * fillPrice * ForexMaxCommissionsPerOrderAsPercent / 100,
				ForexPerSidePerUnitCommission * fillQuantity / 1e5
			}.Min()),
			InstrumentType.Future => FuturesPerSidePerContractCommission * fillQuantity,
			InstrumentType.CryptoCurrency => Math.Max(MinCryptoCommission, symbolInfo.PointValue * fillPrice * fillQuantity * PercentCryptoCommission / 100),
			_ => 0
		};
	}
}
