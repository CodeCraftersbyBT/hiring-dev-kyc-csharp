using KYC.Models;

namespace KYC.Services;

public class KYCService
{
    public KYCCheckResult CheckCustomer(Customer customer)
    {
        var result = new KYCCheckResult
        {
            CustomerId = customer.Identifier
        };

        var riskScore = 0;

        if (customer.AddressCountryCode != "RO")
            riskScore += 20;
        if (customer.Category == CustomerCategory.Private)
            riskScore += 30;

        if (customer.Reputations != null && customer.Reputations.Any())
        {
            var modules = new List<string>();
            foreach (var reputation in customer.Reputations)
            {
                if (reputation.ModuleName == "BL" && reputation.MatchRate > 0.4m)
                    riskScore += 60;
                if (reputation.ModuleName == "SI")
                    riskScore += 100;
                
                if (!modules.Contains(reputation.ModuleName))
                    modules.Add(reputation.ModuleName);                
            }
            if(modules.Count > 3)
                riskScore += 10;
        }

        result.RiskScore = riskScore;
        if (customer.Type == CustomerType.PF && riskScore < 50)
            result.Acceptable = true;
        if (customer.Type == CustomerType.PJ && riskScore <= 50)
            result.Acceptable = true;

        return result;
    }
}