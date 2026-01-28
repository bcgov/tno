# AutoClipper Harness

Standalone console app for local testing of the AutoClipper pipeline.

## Usage

```bash
dotnet run --project tools/auto-clipper-harness -- <media-file> [language] [outputDir]
```

## Configuration

Copy `.env.sample` to `.env` and fill in your keys.

### Provider Selection

```bash
AUTOCLIP_HARNESS_PROVIDER=azure_speech         # default
AUTOCLIP_HARNESS_PROVIDER=azure_video_indexer  # with speaker identification
```

### Azure Speech (default)

- `AUTOCLIP_HARNESS_SPEECH_KEY` / `AUTOCLIP_HARNESS_SPEECH_REGION`
- `AUTOCLIP_HARNESS_STORAGE_CONNECTION_STRING` / `AUTOCLIP_HARNESS_STORAGE_CONTAINER`

### Azure Video Indexer

- `AUTOCLIP_HARNESS_VI_ACCOUNT_ID` / `AUTOCLIP_HARNESS_VI_API_KEY`
- `AUTOCLIP_HARNESS_VI_LOCATION` (default: trial)
- `AUTOCLIP_HARNESS_VI_PERSON_MODELS` - JSON map for multiple Person Models
- `AUTOCLIP_HARNESS_VI_PERSON_MODEL_KEY` - which model to use

### LLM

- `AUTOCLIP_HARNESS_LLM_ENDPOINT` / `AUTOCLIP_HARNESS_LLM_KEY`

## Output

- `clip_XX.mp4` / `clip_XX.txt` - segmented clips with transcripts
- `transcript_full.txt` - full transcript (with speaker labels if enabled)
- `video_indexer_raw_response.json` - raw API response (Video Indexer only)
- `llm_prompt_debug.txt` - LLM prompt for debugging

## Notes

- Video Indexer uploads original media directly; Azure Speech requires WAV conversion
- Requires `ffmpeg` on PATH for clip extraction
