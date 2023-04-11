using KYC.Models;
using System.Security.Cryptography.X509Certificates;

namespace KYC.Services;

public class KYCService
{
    public KYCCheckResult CheckCustomer(Customer customer)
    {
        var result = new KYCCheckResult
        {
            CustomerId = customer.Identifier,
            CheckPerfomred = true
       };

        if (customer.LastCheck > DateTime.Today.AddMonths(-3))
        {
            result.CheckPerfomred = false;
            return result;
        }

        var riskScore = 0;

        if (customer.AddressCountryCode != "RO")
            riskScore += 20;
        if (customer.Category == CustomerCategory.Private)
            riskScore += 30;

        if(customer.IsResident &&  customer.AddressCountryCode != "RO")
            riskScore += 20;
        if (!customer.IsResident && customer.AddressCountryCode != "RO")
            riskScore += 30;

        if (customer.Reputations != null && customer.Reputations.Any())
        {
            var reputations = new List<Reputation>();
            foreach (var reputation in customer.Reputations)
            {
                var containsReputation = reputations.Any(r => r.ModuleName == reputation.ModuleName 
                                                           && r.MatchRate == reputation.MatchRate);

                if (reputation.ModuleName == "BL" && reputation.MatchRate > 0.4m && !containsReputation)
                    riskScore += 60;
                if (reputation.ModuleName == "SI" && !containsReputation)
                    riskScore += 100;
                
                if (!containsReputation)
                    reputations.Add(reputation);                
            }
            if(reputations.Count > 3)
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