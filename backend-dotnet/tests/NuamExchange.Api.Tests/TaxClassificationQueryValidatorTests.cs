using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Tests;

public sealed class TaxClassificationQueryValidatorTests
{
    private readonly TaxClassificationQueryValidator validator = new();

    [Fact]
    public void Validate_WithEmptyQuery_AppliesDefaultPaginationAndOrdering()
    {
        var result = validator.Validate(new TaxClassificationQuery(null, null, null, null));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Query!.Page);
        Assert.Equal(20, result.Query.PageSize);
        Assert.Equal("validFrom", result.Query.SortBy);
        Assert.Equal("desc", result.Query.SortDirection);
    }

    [Fact]
    public void Validate_WithPageLowerThanOne_Fails()
        => Assert.False(validator.Validate(new TaxClassificationQuery(null, null, null, null, Page: 0)).Succeeded);

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithInvalidPageSize_Fails(int pageSize)
        => Assert.False(validator.Validate(new TaxClassificationQuery(null, null, null, null, PageSize: pageSize)).Succeeded);

    [Fact]
    public void Validate_WithInvalidSortDirection_Fails()
        => Assert.False(validator.Validate(new TaxClassificationQuery(null, null, null, null, SortDirection: "sideways")).Succeeded);

    [Fact]
    public void Validate_WithSortByOutsideWhitelist_Fails()
        => Assert.False(validator.Validate(new TaxClassificationQuery(null, null, null, null, SortBy: "creatorUser.passwordHash")).Succeeded);

    [Fact]
    public void Validate_WithTextFilters_TrimsAndConvertsBlankValuesToNull()
    {
        var result = validator.Validate(new TaxClassificationQuery("  bono  ", "  BCS  ", 2026, "   ", SortBy: " TAXPERIOD ", SortDirection: " ASC "));

        Assert.True(result.Succeeded);
        Assert.Equal("bono", result.Query!.Search);
        Assert.Equal("BCS", result.Query.Market);
        Assert.Null(result.Query.Status);
        Assert.Equal("taxPeriod", result.Query.SortBy);
        Assert.Equal("asc", result.Query.SortDirection);
    }
}
