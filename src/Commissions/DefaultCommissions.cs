namespace Tickblaze.Scripts.Commissions;

public class DefaultCommissions : CommissionSimulator
{
	private const string PerOrderDescription = "The commission paid per submitted order";

	private const string PerUnitDescription = "The commission paid on an order per each one quantity bought or sold";

	[Parameter("Commission Per Order", GroupName = "Stocks", Description = PerOrderDescription)]
	[NumericRange(0, double.MaxValue)]
	public double StockPerOrderCommission { get; set; } = 10;

	[Parameter("Commission Per Quantity", GroupName = "Stocks", Description = PerUnitDescription)]
	[NumericRange(0, double.MaxValue)]
	public double StockPerUnitCommission { get; set; } = 0.01;

	[Parameter("Commission Per Order", GroupName = "Futures", Description = PerOrderDescription)]
	[NumericRange(0, double.MaxValue)]
	public double FuturesPerOrderCommission { get; set; } = 0.25;

	[Parameter("Commission Per Quantity", GroupName = "Futures", Description = PerUnitDescription)]
	[NumericRange(0, double.MaxValue)]
	public double FuturesPerUnitCommission { get; set; } = 1.5;

	[Parameter("Commission Per Order", GroupName = "Forex", Description = PerOrderDescription)]
	[NumericRange(0, double.MaxValue)]
	public double ForexPerOrderCommission { get; set; } = 1;

	[Parameter("Commission Per Quantity", GroupName = "Forex", Description = PerUnitDescription)]
	[NumericRange(0, double.MaxValue)]
	public double ForexPerUnitCommission { get; set; } = 0.00002;

	[Parameter("Commission Per Order", GroupName = "Crypto", Description = PerOrderDescription)]
	[NumericRange(0, double.MaxValue)]
	public double CryptoPerOrderCommission { get; set; } = 1;

	[Parameter("Commission Per Quantity", GroupName = "Crypto", Description = PerUnitDescription)]
	[NumericRange(0, double.MaxValue)]
	public double CryptoPerUnitCommission { get; set; } = 0.01;

	public DefaultCommissions()
	{
		Name = "Per Order/Quantity";
		Description = "Adds simulated per order and per unit traded commissions to orders";
	}

	public override double Calculate(Symbol symbolInfo, double fillQuantity, double fillPrice, bool isFirstFill)
	{
		var perOrderCommission = symbolInfo.Type switch
		{
			InstrumentType.ETF or InstrumentType.Stock or InstrumentType.Index => StockPerOrderCommission,
			InstrumentType.Forex => ForexPerOrderCommission,
			InstrumentType.Future => FuturesPerOrderCommission,
			InstrumentType.CryptoCurrency => CryptoPerOrderCommission,
			_ => 0
		};

		var perQuantityCommission = symbolInfo.Type switch
		{
			InstrumentType.ETF or InstrumentType.Stock or InstrumentType.Index => StockPerUnitCommission,
			InstrumentType.Forex => ForexPerUnitCommission,
			InstrumentType.Future => FuturesPerUnitCommission,
			InstrumentType.CryptoCurrency => CryptoPerUnitCommission,
			_ => 0
		};
		
		return perOrderCommission + fillQuantity * perQuantityCommission;
	}
}
