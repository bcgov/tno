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

Supports two authentication modes: **API Key** (Trial/Dev) and **ARM Authentication** (Production).

#### 1. API Key Authentication (Trial / Simple)
Required for local development or trial accounts.

- `Service__AzureVideoIndexerAccountId` - Video Indexer Account ID
- `Service__AzureVideoIndexerLocation` - Account location (for trial: `trial`)
- `Service__AzureVideoIndexerApiKey` - API subscription key (required in this mode)

#### 2. ARM Authentication (Production / Recommended)
Uses a Service Principal. ARM mode is enabled automatically when all three values are present:

- `Service__AzureVideoIndexerArmTenantId`
- `Service__AzureVideoIndexerArmClientId`
- `Service__AzureVideoIndexerArmClientSecret`

Additional required values for ARM mode:

- `Service__AzureVideoIndexerSubscriptionId` - Azure Subscription ID
- `Service__AzureVideoIndexerResourceGroup` - Resource Group Name
- `Service__AzureVideoIndexerAccountName` - Video Indexer resource name in Azure
- `Service__AzureVideoIndexerAccountId` - Video Indexer Account ID used by upload/index APIs
- `Service__AzureVideoIndexerLocation` - Actual region (for example `canadacentral`), not `trial`

API key behavior in ARM mode:

- `Service__AzureVideoIndexerApiKey` is ignored when ARM mode is enabled.
- You can leave it empty in ARM mode.

Important consistency note:

- `AccountName`, `AccountId`, and `Location` must refer to the same Video Indexer account.

#### General Settings
- `Service__AzureVideoIndexerTimeoutMinutes` (default: 60) - Max wait time for processing
- `Service__AzureVideoIndexerPollingIntervalSeconds` (default: 30) - Status check interval

#### OpenShift Secret Keys (`azure-video-indexer`)
If you deploy via `openshift/kustomize/services/auto-clipper`, configure these keys in the
`azure-video-indexer` secret (typically from `video-indexer.env` in each overlay):

- `AZURE_VIDEO_INDEXER_ACCOUNT_ID`
- `AZURE_VIDEO_INDEXER_LOCATION`
- `AZURE_VIDEO_INDEXER_API_KEY` (required only for API Key mode; ignored in ARM mode)
- `AZURE_VIDEO_INDEXER_ARM_TENANT_ID`
- `AZURE_VIDEO_INDEXER_ARM_CLIENT_ID`
- `AZURE_VIDEO_INDEXER_ARM_CLIENT_SECRET`
- `AZURE_VIDEO_INDEXER_SUBSCRIPTION_ID`
- `AZURE_VIDEO_INDEXER_RESOURCE_GROUP`
- `AZURE_VIDEO_INDEXER_ACCOUNT_NAME`

### LLM & General

- Service\_\_LlmApiUrl, Service\_\_LlmApiKey, Service\_\_LlmDeployment, Service\_\_LlmApiVersion
- Service\_\_StationConfigPath (optional override for station YAML directory)