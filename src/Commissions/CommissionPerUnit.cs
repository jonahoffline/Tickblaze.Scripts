using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.Commissions;

/// <summary>
/// This commission script calculates the commission paid for an executed order, determined by its quantity and denominated in the account currency.
/// </summary>
public sealed class CommissionPerUnit : Commission
{
	[Parameter("Commission per unit", Description = "The commission amount paid for each unit (share / contract  / lot) of the executed order.")]
	public double CommissionValuePerUnit { get; set; } = 0.01;

	public CommissionPerUnit()
	{
		Name = "Commission per Unit (new)";
		Description = "This commission script calculates the commission paid for an executed order, determined by its quantity and denominated in the account currency.";
	}

	protected override double OnCommission(int strategyNumber, IOrder order, int fillIndex, double fillQuantity, double fillPrice)
	{
		if (fillIndex > 0)
		{
			return 0;
		}

		return CommissionValuePerUnit * fillQuantity;
	}
}
