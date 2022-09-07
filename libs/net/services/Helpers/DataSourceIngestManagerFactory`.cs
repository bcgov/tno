using Microsoft.Extensions.DependencyInjection;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// DataSourceIngestManagerFactory class, provides a way to create DataSourceIngestManager objects.
/// </summary>
public class DataSourceIngestManagerFactory<TDataSourceIngestManager, TOption>
    where TDataSourceIngestManager : IDataSourceIngestManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceIngestManagerFactory object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public DataSourceIngestManagerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a new instance of a DataSourceIngestManager object.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public TDataSourceIngestManager Create(DataSourceModel dataSource)
    {
        // var type = typeof(TDataSourceIngestManager).MakeGenericType(new[] { typeof(DataSourceModel), typeof(IIngestAction<TOption>), typeof(IApiService), typeof(ILogger<TDataSourceIngestManager>) });]
        var type = typeof(TDataSourceIngestManager);
        var con = type.GetConstructors().First();

        var args = new List<object>();
        foreach (var cparam in con.GetParameters())
        {
            if (cparam.ParameterType == typeof(DataSourceModel))
                args.Add(dataSource);
            else
                args.Add(_serviceProvider.GetRequiredService(cparam.ParameterType));
        }

        return (TDataSourceIngestManager)(Activator.CreateInstance(type, args.ToArray())
            ?? throw new InvalidOperationException($"Unable to create instance of type '{type.Name}'"));
    }
    #endregion
}
