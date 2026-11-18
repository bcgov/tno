using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

/// <summary>
/// Saving a stale admin product form must be rejected instead of changing the report or notification subscriptions
/// that were changed after it was loaded.
/// </summary>
public class ProductServiceTest : IDisposable
{
    #region Variables
    readonly TestHelper helper = new();
    #endregion

    #region Constructor
    public ProductServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext("product-service");
            services.AddPrincipalForRole("editor");
            services.AddOptions();
            services.Configure<ChesOptions>(o => { });
            services.AddMockSingleton<IChesService>();
            services.AddMockSingleton<ILogger<ProductService>>();
            services.AddSingleton<IProductService, ProductService>();
        });
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Seed a report product that alice subscribes to, after which an admin unsubscribed her from the report.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static Product SeedProduct(TNOContext context)
    {
        var owner = new User("owner", "owner@test.com") { Id = 1 };
        var alice = new User("alice", "alice@test.com") { Id = 2 };
        context.AddRange(owner, alice);

        var template = new ReportTemplate("template", ReportType.Content, "subject", "body") { Id = 1 };
        context.Add(template);
        context.Add(new Report(1, "report", template.Id, owner.Id));

        var product = new Product(1, "product", ProductType.Report, 1);
        context.Add(product);
        context.Add(new UserProduct(alice.Id, product.Id));
        context.Add(new UserReport(alice.Id, 1, true));
        context.SaveChanges();

        // The admin unsubscribes alice from the report, version 1.
        var subscription = context.UserReports.Single(ur => ur.UserId == alice.Id && ur.ReportId == 1);
        subscription.IsSubscribed = false;
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return product;
    }

    private static Product SubmitSubscription(Product product, bool isSubscribed, long? expectedVersion)
    {
        var submitted = new Product(product.Id, product.Name, product.ProductType, product.TargetProductId);
        var userProduct = new UserProduct(2, product.Id)
        {
            User = new User(new UserReport(2, product.TargetProductId, isSubscribed) { ExpectedVersion = expectedVersion })
        };
        submitted.SubscribersManyToMany.Add(userProduct);
        return submitted;
    }

    private static UserReport GetSubscription(TNOContext context)
    {
        context.ChangeTracker.Clear();
        return context.UserReports.Single(ur => ur.UserId == 2 && ur.ReportId == 1);
    }
    #endregion

    #region Methods
    [Fact]
    public void Update_StaleSubscription_ThrowsConcurrencyAndDoesNotResubscribe()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IProductService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var product = SeedProduct(context);

        // The product form was loaded before the admin unsubscribed alice, version 0.
        var stale = SubmitSubscription(product, true, 0);

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
        Assert.False(GetSubscription(context).IsSubscribed);
    }

    [Fact]
    public void Update_CurrentSubscription_AppliesChange()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IProductService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var product = SeedProduct(context);

        // The product form was loaded after the admin unsubscribed alice, version 1, and subscribes her again.
        var current = SubmitSubscription(product, true, 1);

        // Act
        service.UpdateAndSave(current);

        // Assert
        Assert.True(GetSubscription(context).IsSubscribed);
    }

    [Fact]
    public void Update_SubscriptionAddedAfterLoad_ThrowsConcurrencyAndKeepsSubscription()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IProductService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var product = SeedProduct(context);

        // The form loaded bob as a product subscriber without a report subscription.
        context.Add(new User("bob", "bob@test.com") { Id = 3 });
        context.Add(new UserProduct(3, product.Id));
        context.SaveChanges();

        // An admin subscribes bob to the report after the form loaded.
        context.Add(new UserReport(3, product.TargetProductId, true));
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var stale = new Product(product.Id, product.Name, product.ProductType, product.TargetProductId);
        stale.SubscribersManyToMany.Add(new UserProduct(3, product.Id)
        {
            User = new User(new UserReport(3, product.TargetProductId, false))
        });

        // Act / Assert
        Assert.Throws<DbUpdateConcurrencyException>(() => service.UpdateAndSave(stale));
        context.ChangeTracker.Clear();
        Assert.True(context.UserReports.Single(ur => ur.UserId == 3 && ur.ReportId == product.TargetProductId).IsSubscribed);
    }

    [Fact]
    public void Update_NewProductSubscriberWithExistingSubscription_AppliesChange()
    {
        // Arrange
        var service = helper.Provider.GetRequiredService<IProductService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var product = SeedProduct(context);

        // Bob is not a product subscriber, but has an unsubscribed report subscription the form never loaded.
        context.Add(new User("bob", "bob@test.com") { Id = 3 });
        context.Add(new UserReport(3, product.TargetProductId, false));
        context.SaveChanges();
        context.ChangeTracker.Clear();

        // The admin adds bob to the product, alice is returned unchanged with the version that was loaded.
        var submitted = SubmitSubscription(product, false, 1);
        submitted.SubscribersManyToMany.Add(new UserProduct(3, product.Id)
        {
            User = new User(new UserReport(3, product.TargetProductId, true))
        });

        // Act
        service.UpdateAndSave(submitted);
        context.ChangeTracker.Clear();

        // Assert
        Assert.True(context.UserReports.Single(ur => ur.UserId == 3 && ur.ReportId == product.TargetProductId).IsSubscribed);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        var context = helper.Provider.GetRequiredService<TNOContext>();
        context.EnsureDeleted();
        context.Dispose();
    }
    #endregion
}
