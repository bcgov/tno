# AutoClipper Service

The AutoClipper service consumes clip requests from Kafka, transcribes media using Azure Speech or Azure Video Indexer,
and segments the transcript into clips using a boundary-aware LLM workflow boosted by station heuristics. Key concepts:

- **Station profiles** (Config/Stations/\*.yml) define transcription provider, language, sample rate, heuristic keywords,
  custom prompts, and category mappings for weather/traffic/ads.
- **Transcription providers**:
  - `azure_speech` (default) - Fast batch transcription, outputs plain text.
  - `azure_video_indexer` - Supports speaker identification (speaker1:, speaker2:, or named via Person Model).
- **Pipeline** (ClipProcessingPipeline) selects the transcription provider based on station config, transcribes the media,
  and feeds transcripts plus station config into ClipSegmentationService.
- **Segmentation** uses Azure OpenAI to score story boundaries, merges in regex-based heuristics, snaps clips to transcript
  sentences, and tags each clip with a category before AutoClipperManager creates content and uploads the media.

## Development

1. Update station YAMLs under Config/Stations (copy CKNW.yml for Azure Speech, CHAN.yml for Video Indexer).
2. Run `dotnet build services/net/auto-clipper/TNO.Services.AutoClipper.csproj` to verify changes.
3. Use the harness (see tools/auto-clipper-harness/README.md) to manually validate segmentation on sample media.

## Configuration

### Azure Speech (default provider)

- Service\_\_AzureSpeechKey / Service\_\_AzureSpeechRegion
- Service\_\_AzureSpeechStorageConnectionString / Service\_\_AzureSpeechStorageContainer (batch upload destination)
- Service\_\_AzureSpeechBatchEndpoint, Service\_\_AzureSpeechBatchApiVersion, Service\_\_AzureSpeechBatchPollingIntervalSeconds, Service\_\_AzureSpeechBatchTimeoutMinutes, Service\_\_AzureSpeechStorageSasExpiryMinutes (optional batch tuning)

### Azure Video Indexer (optional, for speaker identification)

- Service\_\_AzureVideoIndexerAccountId - Your Video Indexer account ID
- Service\_\_AzureVideoIndexerLocation - Account location (e.g., `trial`, `eastus`)
- Service\_\_AzureVideoIndexerApiKey - API subscription key
- Service\_\_AzureVideoIndexerTimeoutMinutes (default: 60) - Max wait time for processing
- Service\_\_AzureVideoIndexerPollingIntervalSeconds (default: 30) - Status check interval

### LLM & General

- Service\_\_LlmApiUrl, Service\_\_LlmApiKey, Service\_\_LlmDeployment, Service\_\_LlmApiVersion
- Service\_\_StationConfigPath (optional override for station YAML directory)