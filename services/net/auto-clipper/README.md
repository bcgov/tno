# AutoClipper Service

The AutoClipper service consumes clip requests from Kafka, normalizes audio, transcribes it with Azure Speech, and
segments the transcript into clips using a boundary-aware LLM workflow boosted by station heuristics. Key concepts:

- **Station profiles** (Config/Stations/\*.yml) define language, sample rate, heuristic keywords, custom prompts, and
  category mappings for weather/traffic/ads.
- **Pipeline** (ClipProcessingPipeline) normalizes audio, transcribes via AzureSpeechTranscriptionService, and feeds
  transcripts plus station config into ClipSegmentationService.
- **Segmentation** uses Azure OpenAI to score story boundaries, merges in regex-based heuristics, snaps clips to transcript
  sentences, and tags each clip with a category before AutoClipperManager creates content and uploads the media.

## Development

1. Update station YAMLs under Config/Stations (copy CKNW.yml as a starting point).
2. Run dotnet build services/net/auto-clipper/TNO.Services.AutoClipper.csproj to verify changes.
3. Use the harness (see tools/auto-clipper-harness/README.md) to manually validate segmentation on sample audio.

## Configuration

Important Service\_\_ env vars:

- Service**AzureSpeechKey / Service**AzureSpeechRegion
- Service**LlmApiUrl, Service**LlmApiKey, Service**LlmDeployment, Service**LlmApiVersion
- Service\_\_StationConfigPath (optional override for station YAML directory)
