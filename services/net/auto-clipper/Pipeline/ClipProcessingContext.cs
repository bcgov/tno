using TNO.Kafka.Models;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.Pipeline;

public record ClipProcessingContext(string SourcePath, StationProfile StationProfile, ClipRequestModel Request, int TargetSampleRate);
