using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TNO.Services.AutoClipper.Config;

public interface IStationConfigurationService
{
    StationProfile GetProfile(string? stationCode);
}

public class StationConfigurationService : IStationConfigurationService
{
    private readonly ILogger<StationConfigurationService> _logger;
    private readonly ConcurrentDictionary<string, StationProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly StationProfile _defaultProfile = new();

    public StationConfigurationService(IOptions<AutoClipperOptions> options, ILogger<StationConfigurationService> logger)
    {
        _logger = logger;
        LoadProfiles(options.Value.StationConfigPath);
    }

    public StationProfile GetProfile(string? stationCode)
    {
        if (!string.IsNullOrWhiteSpace(stationCode) && _profiles.TryGetValue(stationCode, out var profile))
            return profile;
        if (_profiles.TryGetValue("default", out var defaultProfile))
            return defaultProfile;
        return _defaultProfile;
    }

    private void LoadProfiles(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) { _logger.LogWarning("StationConfigPath not set; using defaults"); return; }
        var basePath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        if (Directory.Exists(basePath))
        {
            foreach (var file in Directory.EnumerateFiles(basePath, "*.yml", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(basePath, "*.yaml", SearchOption.TopDirectoryOnly)))
            {
                TryLoadProfile(file);
            }
        }
        else if (File.Exists(basePath))
        {
            TryLoadProfile(basePath);
        }
        else
        {
            _logger.LogWarning("Station configuration path '{path}' not found.", basePath);
        }
    }

    private void TryLoadProfile(string file)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            using var reader = File.OpenText(file);
            var profile = deserializer.Deserialize<StationProfile>(reader);
            if (string.IsNullOrWhiteSpace(profile.Name)) profile.Name = Path.GetFileNameWithoutExtension(file);
            _profiles[profile.Name] = profile;
            _logger.LogInformation("Loaded station profile '{Name}' from {File}", profile.Name, file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load station profile from {File}", file);
        }
    }
}
