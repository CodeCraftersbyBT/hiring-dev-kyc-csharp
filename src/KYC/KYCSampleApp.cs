// See https://aka.ms/new-console-template for more information

using KYC.Models;
using KYC.Services;

var kycService = new KYCService();

var customer = new Customer
{
    Identifier = "dfstyj053o4rgwh4yt",
    AddressCountryCode = "HU",
    IsResident = false,
    Category = CustomerCategory.Retail,
    Reputations = new List<Reputation>
    {
        new() { ModuleName = "BL", MatchRate = 0.41m }
    }
};

var result = kycService.CheckCustomer(customer);


Console.WriteLine($"Customer is acceptable: {result.Acceptable}");
Console.WriteLine($"Customer risk score: {result.RiskScore}");
