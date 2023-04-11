namespace KYC.Models;

public class Customer
{
    public string Identifier { get; set; } = null!;
    public CustomerType Type { get; set; }
    public CustomerCategory Category { get; set; }
    public string AddressCountryCode { get; set; } = null!;
    public bool IsResident { get; set; }
    public IList<Reputation>? Reputations { get; set; }
    public DateTime LastCheck { get; set; }
}