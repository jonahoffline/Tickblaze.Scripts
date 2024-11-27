using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.Commissions;

/// <summary>
/// This commission script calculates the constant commission paid for each executed order, denominated in the account currency.
/// </summary>
public sealed class CommissionPerOrder : Commission
{
	[Parameter("Commission per order", Description = "The commission amount paid for each executed order.")]
	public double CommissionValuePerOrder { get; set; } = 10;

	public CommissionPerOrder()
	{
		Name = "Commission per Order";
		Description = "This commission script calculates the constant commission paid for each executed order, denominated in the account currency.";
	}

	protected override double OnCommission(int strategyNumber, IOrder order, int fillIndex, double fillQuantity, double fillPrice)
	{
		if (fillIndex > 0)
		{
			return 0;
		}

		return CommissionValuePerOrder;
	}
}
