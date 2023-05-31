using Microsoft.Extensions.DependencyInjection;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// IngestManagerFactory class, provides a way to create IngestManager objects.
/// </summary>
public class IngestManagerFactory<TIngestServiceActionManager, TOption>
    where TIngestServiceActionManager : IIngestServiceActionManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestManagerFactory object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public IngestManagerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a new instance of a IngestManager object.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public TIngestServiceActionManager Create(IngestModel ingest, IServiceScope? serviceScope = null)
    {
        // var type = typeof(TIngestManager).MakeGenericType(new[] { typeof(IngestModel), typeof(IIngestAction<TOption>), typeof(IApiService), typeof(ILogger<TIngestManager>) });]
        var type = typeof(TIngestServiceActionManager);
        var con = type.GetConstructors().First();

        var args = new List<object>();
        foreach (var cparam in con.GetParameters())
        {
            if (cparam.ParameterType == typeof(IngestModel))
                args.Add(ingest);
            else if (serviceScope != null)
            {
                args.Add(serviceScope.ServiceProvider.GetRequiredService(cparam.ParameterType));

            } else
                args.Add(_serviceProvider.GetRequiredService(cparam.ParameterType));
        }

        return (TIngestServiceActionManager)(Activator.CreateInstance(type, args.ToArray())
            ?? throw new InvalidOperationException($"Unable to create instance of type '{type.Name}'"));
    }
    #endregion
}
