using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.Commissions;

/// <summary>
/// This commission script calculates the commission paid for an executed order, determined by a constant amount in addition to a variable amount based on the order quantity.
/// </summary>
public sealed class CommissionPerOrderAndUnit : Commission
{
	[Parameter("Commission per order", Description = "The commission amount paid for each executed order.")]
	public double CommissionValuePerOrder { get; set; } = 10;

	[Parameter("Commission per unit", Description = "The commission amount paid for each unit (share / contract  / lot) of the executed order.")]
	public double CommissionValuePerUnit { get; set; } = 0.01;

	public CommissionPerOrderAndUnit()
	{
		Name = "Commission per Order and Unit";
		Description = "This commission script calculates the commission paid for an executed order, determined by a constant amount in addition to a variable amount based on the order quantity.";
	}

	protected override double OnCommission(int strategyNumber, IOrder order, int fillIndex, double fillQuantity, double fillPrice)
	{
		if (fillIndex > 0)
		{
			return 0;
		}

		return CommissionValuePerOrder + (CommissionValuePerUnit * fillQuantity);
	}
}
