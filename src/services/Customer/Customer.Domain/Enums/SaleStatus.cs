using System.ComponentModel;
namespace Customer.Domain.Enums
{
    public enum SaleStatus
    {
        [Description("LOST")] Lost = 1,
        [Description("COLD")] Cold,
        [Description("WARM")] Warm,
        [Description("HOT")] Hot,
        [Description("CAPTURED")] Captured,
        [Description("WILL")] Will,
    }
}
