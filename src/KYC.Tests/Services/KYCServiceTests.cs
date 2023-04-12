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
    [InlineData("RO", CustomerCategory.AGR, CustomerType.PF, true, true, 0)]
    [InlineData("BR", CustomerCategory.Retail, CustomerType.PF,true, true, 40)]
    [InlineData("BR", CustomerCategory.Retail, CustomerType.PF, false, false, 50)]
    [InlineData("IT", CustomerCategory.Private, CustomerType.PF,false, false, 80)]
    [InlineData("IT", CustomerCategory.Private, CustomerType.PJ,true, false, 70)]
    [InlineData("RO", CustomerCategory.Private, CustomerType.PF, true, true, 30)]
    [InlineData("RO", CustomerCategory.Private, CustomerType.PF, false, true, 30)]
    [InlineData("RO", CustomerCategory.Private, CustomerType.PJ, true, true, 30)]
    [InlineData("RO", CustomerCategory.Private, CustomerType.PJ, false, true, 30)]
    public void WhenCustomerDoesNotHaveReputations_ForGivenCustomerData_ReturnTheExpectedResult(
        string addressCountryCode,
        CustomerCategory category,
        CustomerType type,
        bool isResident,
        bool expectedAcceptable,
        int expectedRiskScore)
    {
        var customer = new Customer
        {
            AddressCountryCode = addressCountryCode,
            Category = category,
            Type = type,
            IsResident = isResident,            
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
    [MemberData(nameof(BLReputationScenarios))]
    public void WhenCustomerHasMultipleBLReputation_ReturnExpectedResult(
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
                new() { ModuleName = "BL", MatchRate = matchRate },
                new() { ModuleName = "BL", MatchRate = matchRate },
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
    public void WhenCustomerHasMultipleSIReputation_ReturnExpectedResult(CustomerType type)
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
                new() { ModuleName = "SI" },
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

    [Theory]
    [InlineData("2022-3-10", true)]
    [InlineData("2023-4-1", false)]
    public void LastTimeChecked_ReturnExpectedResult(
        DateTime dateTime,
        bool checkPerformed)
    {
        var customer = new Customer
        {
            AddressCountryCode = "RO",
            Category = CustomerCategory.Retail,
            Type = CustomerType.PF,
            Reputations = new List<Reputation>
            {
                new() { ModuleName = "SI" }
            },
            LastCheck = dateTime
        };

        var result = _sut.CheckCustomer(customer);

        Assert.Equal(checkPerformed, result.CheckPerfomred);
    }

    [Theory]
    [InlineData(1, true, 10)]
    [InlineData(3, true, 30)]
    [InlineData(7, false, 70)]
    public void WhenCustomerHasRSS_Reputations_ReturnExpectedResult(
        int numerOfRSSReputations,
        bool expectedAcceptable,
        int expectedRiskScore)
    {


        var customer = new Customer
        {
            AddressCountryCode = "RO",
            Category = CustomerCategory.Retail,
            Type = CustomerType.PF,
            Reputations = new List<Reputation>()
        };

        for (int i = 0; i < numerOfRSSReputations; i++)
        {
            customer.Reputations.Add(
                new() { ModuleName = "RSS_test" }
            );
        }

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