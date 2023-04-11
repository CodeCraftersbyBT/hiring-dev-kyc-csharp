using KYC.Models;
using KYC.Services;

namespace KYC.Tests.Services;

public class KYCServiceTests
{
    private readonly KYCService _sut;

    public KYCServiceTests()
    {
        _sut = new KYCService();
    }

    [Theory]
    [InlineData("RO", CustomerCategory.AGR, CustomerType.PF, true, 0)]
    [InlineData("BR", CustomerCategory.Retail, CustomerType.PF, true, 20)]
    [InlineData("IT", CustomerCategory.Private, CustomerType.PF, false, 50)]
    [InlineData("IT", CustomerCategory.Private, CustomerType.PJ, true, 50)]
    [InlineData("RO", CustomerCategory.Private, CustomerType.PF, true, 30)]
    [InlineData("RO", CustomerCategory.Private, CustomerType.PJ, true, 30)]
    public void WhenCustomerDoesNotHaveReputations_ForGivenCustomerData_ReturnTheExpectedResult(
        string addressCountryCode,
        CustomerCategory category,
        CustomerType type,
        bool expectedAcceptable,
        int expectedRiskScore)
    {
        var customer = new Customer
        {
            AddressCountryCode = addressCountryCode,
            Category = category,
            Type = type
        };

        var result = _sut.CheckCustomer(customer);

        Assert.Equal(expectedAcceptable, result.Acceptable);
        Assert.Equal(expectedRiskScore, result.RiskScore);
    }

    [Theory]
    [MemberData(nameof(BLReputationScenarios))]
    public void WhenCustomerHasBLReputation_ReturnExpectedResult(
        decimal matchRate,
        bool expectedAcceptable,
        int expectedRiskScore)
    {
        var customer = new Customer
        {
            AddressCountryCode = "RO",
            Category = CustomerCategory.Retail,
            Type = CustomerType.PF,
            Reputations = new List<Reputation>
            {
                new() { ModuleName = "BL", MatchRate = matchRate }
            }
        };

        var result = _sut.CheckCustomer(customer);

        Assert.Equal(expectedAcceptable, result.Acceptable);
        Assert.Equal(expectedRiskScore, result.RiskScore);
    }

    [Theory]
    [InlineData(CustomerType.PF)]
    [InlineData(CustomerType.PJ)]
    public void WhenCustomerHasSIReputation_ReturnExpectedResult(CustomerType type)
    {
        const bool expectedAcceptable = false;
        const int expectedRiskScore = 100;

        var customer = new Customer
        {
            AddressCountryCode = "RO",
            Category = CustomerCategory.Retail,
            Type = type,
            Reputations = new List<Reputation>
            {
                new() { ModuleName = "SI" }
            }
        };

        var result = _sut.CheckCustomer(customer);

        Assert.Equal(expectedAcceptable, result.Acceptable);
        Assert.Equal(expectedRiskScore, result.RiskScore);
    }

    [Theory]
    [InlineData(CustomerType.PF)]
    [InlineData(CustomerType.PJ)]
    public void WhenCustomerHasMoreThanThreDiffrenetModules_ReturnExpectedResult(CustomerType type)
    {
        const bool expectedAcceptable = false;
        const int expectedRiskScore = 110;

        var customer = new Customer
        {
            AddressCountryCode = "RO",
            Category = CustomerCategory.Retail,
            Type = type,
            Reputations = new List<Reputation>
            {
                new() { ModuleName = "SI" },
                new() { ModuleName = "MS" },
                new() { ModuleName = "TS" },
                new() { ModuleName = "BZ" }
            }
        };

        var result = _sut.CheckCustomer(customer);

        Assert.Equal(expectedAcceptable, result.Acceptable);
        Assert.Equal(expectedRiskScore, result.RiskScore);
    }

    public static IEnumerable<object[]> BLReputationScenarios =>
        new List<object[]>
        {
            new object[] { 0.4m, true, 0 },
            new object[] { 0.401m, false, 60 }
        };
}