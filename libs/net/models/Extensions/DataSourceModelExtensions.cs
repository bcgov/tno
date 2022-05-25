using System.Text.Json;
using TNO.API.Areas.Services.Models.DataSource;

namespace TNO.Models.Extensions;

/// <summary>
/// DataSourceModelExtensions static class, provies extension methods for DataSourceModel.
/// </summary>
public static class DataSourceModelExtensions
{
    /// <summary>
    /// Get the connection setting value for the specified key.
    /// Or return an empty string if there is no key or value.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetConnectionValue(this DataSourceModel dataSource, string key)
    {
        if (!dataSource.Connection.TryGetValue(key, out object? element)) return "";

        if (element is JsonElement value)
            return String.IsNullOrWhiteSpace(value.GetString()) ? "" : value.GetString()!;

        throw new InvalidOperationException($"Data source connection '{key}' is not a valid JSON element");
    }
}
