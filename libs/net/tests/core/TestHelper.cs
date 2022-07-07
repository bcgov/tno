using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TNO.Test.Core;

public class TestHelper : IDisposable
{
    #region Properties
    public IServiceProvider Provider { get; protected set; }
    protected IServiceCollection Services { get; } = new ServiceCollection();
    #endregion

    #region Constructors
    public TestHelper()
    {
        this.Services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        this.Provider = this.Services.BuildServiceProvider();
    }
    #endregion

    #region Methods
    public IServiceCollection Configure(Action<IServiceCollection> services)
    {
        services(this.Services);
        return this.Services;
    }
    public IServiceProvider Build(Action<IServiceCollection>? services = null)
    {
        services?.Invoke(this.Services);
        this.Provider = this.Services.BuildServiceProvider();
        return this.Provider;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    #endregion
}
