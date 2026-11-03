using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.Test.DAL.Integration;

/// <summary>
/// Tests for the admin filter list's paged search against PostgreSQL: the keyword is matched in the
/// name, description and owner, LIKE wildcards are literal, and the total counts every match.
/// </summary>
[Collection(PostgresCollection.Name)]
public class FilterPageIntegrationTest
{
    #region Variables
    private readonly PostgresFixture _fixture;
    #endregion

    #region Constructors
    public FilterPageIntegrationTest(PostgresFixture fixture)
    {
        _fixture = fixture;
    }
    #endregion

    #region Helpers
    private FilterService CreateService(TNOContext context)
        => new(context, _fixture.Principal, _fixture.Services, TestLogger.For<TagService>());

    private static Filter[] AddFilters(TNOContext context, User owner, string token)
    {
        var filters = new[]
        {
            new Filter(0, $"it-{token}-a name", owner.Id) { Description = "first" },
            new Filter(0, $"it-{token}-b", owner.Id) { Description = $"described {token}" },
            new Filter(0, $"it-{token}-c 100%", owner.Id) { Description = "literal wildcard" },
        };
        context.Filters.AddRange(filters);
        context.SaveChanges();
        return filters;
    }

    private static void Remove(TNOContext context, IEnumerable<Filter> filters)
    {
        context.Filters.RemoveRange(filters);
        context.SaveChanges();
    }
    #endregion

    [PostgresFact]
    public void KeywordMatchesTheNameOrDescriptionAndPagesWithTheTotal()
    {
        using var context = _fixture.CreateContext();
        var owner = context.Users.AsNoTracking().OrderBy(u => u.Id).First();
        var token = Guid.NewGuid().ToString("N")[..12];
        var filters = AddFilters(context, owner, token);
        try
        {
            var service = CreateService(context);

            var first = service.FindPage(new FilterFilter() { Keyword = token.ToUpperInvariant(), Page = 1, Quantity = 2, Sort = ["name"] });
            first.Total.Should().Be(3);
            first.Items.Select(f => f.Name).Should().Equal($"it-{token}-a name", $"it-{token}-b");
            first.Items.First().Owner!.Id.Should().Be(owner.Id);

            var second = service.FindPage(new FilterFilter() { Keyword = token, Page = 2, Quantity = 2, Sort = ["name"] });
            second.Total.Should().Be(3);
            second.Items.Select(f => f.Name).Should().Equal($"it-{token}-c 100%");

            service.FindPage(new FilterFilter() { Keyword = $"described {token}", Page = 1, Quantity = 10 })
                .Items.Select(f => f.Name).Should().Equal($"it-{token}-b");
        }
        finally
        {
            Remove(context, filters);
        }
    }

    [PostgresFact]
    public void KeywordWildcardsAreLiteralAndTheOwnerIsSearched()
    {
        using var context = _fixture.CreateContext();
        var owner = context.Users.AsNoTracking().OrderBy(u => u.Id).First();
        var token = Guid.NewGuid().ToString("N")[..12];
        var filters = AddFilters(context, owner, token);
        try
        {
            var service = CreateService(context);

            // '%' and '_' match themselves, not any text.
            service.FindPage(new FilterFilter() { Keyword = $"{token}-c 100%", Page = 1, Quantity = 10 })
                .Items.Select(f => f.Name).Should().Equal($"it-{token}-c 100%");
            service.FindPage(new FilterFilter() { Keyword = $"{token}_a", Page = 1, Quantity = 10 }).Total.Should().Be(0);

            // The owner's username finds their filters; sorting by the owner's username works.
            var byOwner = service.FindPage(new FilterFilter() { Keyword = owner.Username, Page = 1, Quantity = 500, Sort = ["owner.username desc"] });
            byOwner.Items.Select(f => f.Id).Should().Contain(filters.Select(f => f.Id));
        }
        finally
        {
            Remove(context, filters);
        }
    }
}
