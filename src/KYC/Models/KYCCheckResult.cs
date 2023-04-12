namespace KYC.Models;

public class KYCCheckResult
{
    public string CustomerId { get; set; }
    public int RiskScore { get; set; }
    public bool Acceptable { get; set; }
    public bool CheckPerfomred { get; set; }
}