using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.Commissions;

/// <summary>
/// This commission script calculates the commission paid for an executed stock, futures, forex or crypto order, determined by a constant amount in addition to a variable amount based on the order quantity.
/// </summary>
public sealed class CommissionByInstrument : Commission
{
	[Parameter("Commission per stock order", Description = "The commission amount paid for each executed stock or ETF order.")]
	public double CommissionValuePerStockOrder { get; set; } = 10;

	[Parameter("Commission per stock share", Description = "The commission amount paid for each share of the executed order.")]
	public double CommissionValuePerStockShare { get; set; } = 0.01;

	[Parameter("Commission per futures order", Description = "The commission amount paid for each executed futures or CFD order.")]
	public double CommissionValuePerFuturesOrder { get; set; } = 0.25;

	[Parameter("Commission per futures contract", Description = "The commission amount paid for each futures or CFD contract of the executed order.")]
	public double CommissionValuePerFuturesContract { get; set; } = 1.5;

	[Parameter("Commission per forex order", Description = "The commission amount paid for each executed forex order.")]
	public double CommissionValuePerForexOrder { get; set; } = 1;

	[Parameter("Commission per forex unit", Description = "The commission amount paid for each forex unit of the executed order.")]
	public double CommissionValuePerForexUnit { get; set; } = 0.00002;

	[Parameter("Commission per crypto order", Description = "The commission amount paid for each executed crypto order.")]
	public double CommissionValuePerCryptoOrder { get; set; } = 1;

	[Parameter("Commission per crypto unit", Description = "The commission amount paid for each crypto unit of the executed order.")]
	public double CommissionValuePerCryptoUnit { get; set; } = 0.01;

	public CommissionByInstrument()
	{
		Name = "Commission by Instrument";
		Description = "This commission script calculates the commission paid for an executed stock, futures, forex or crypto order, determined by a constant amount in addition to a variable amount based on the order quantity.";
	}

	protected override double OnCommission(int strategyNumber, IOrder order, int fillIndex, double fillQuantity, double fillPrice)
	{
		if (fillIndex > 0)
		{
			return 0;
		}

		// TODO: Expose Symbol.InstrumentType in the API
		return 0;
	}
}
