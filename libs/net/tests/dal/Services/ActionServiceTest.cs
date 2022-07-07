using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Test.Core;

namespace TNO.Test.DAL;

public class ActionServiceTest : IDisposable
{
    #region Variables
    readonly TestHelper helper = new();
    #endregion

    #region Constructor
    public ActionServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext("action-service");
            services.AddPrincipalForRole("editor");
            services.AddMock<ILogger<ActionService>>();
            services.AddSingleton<IActionService, ActionService>();
        });
    }
    #endregion

    #region Methods
    /// <summary>
    /// Add a new Action to the database.
    /// </summary>
    [Fact]
    public void Add_Success()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var service = helper.Provider.GetRequiredService<IActionService>();
        var context = helper.Provider.GetRequiredService<TNOContext>();

        var action = new Entities.Action("test", Entities.ValueType.Boolean);
        var keys = typeof(Action).GetCacheKeys() ?? Array.Empty<string>();
        var originalCache = context.Cache.Where(c => keys.Contains(c.Key)).ToArray();

        // Act
        service.Add(action);
        var cache = context.Cache.Where(c => keys.Contains(c.Key)).ToArray();

        // Assert
        Assert.Equal(1, action.Id);
        Assert.Equal(0, action.Version);
        Assert.Equal("", action.CreatedBy);
        Assert.Equal(Guid.Empty, action.CreatedById);
        Assert.True(now <= action.CreatedOn);
        Assert.Equal("", action.UpdatedBy);
        Assert.Equal(Guid.Empty, action.UpdatedById);
        Assert.True(now <= action.UpdatedOn);
        Assert.True(originalCache.Length == 0);
        Assert.True(cache.Length == keys.Count());
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
