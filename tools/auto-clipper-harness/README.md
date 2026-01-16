# AutoClipper Harness

The harness is a standalone console app that mirrors the AutoClipper pipeline for manual validation. It
normalizes a local media file, runs Azure Speech transcription, feeds the transcript and station heuristics to the
segmenter, and writes clips/transcripts/prompt debug files for inspection.

## Usage

`dotnet run --project tools/auto-clipper-harness -- <path-to-media> [language] [outputDir]`

- Configure Azure keys and LLM settings via .env (see .env.sample).
- Provide AUTOCLIP_HARNESS_STORAGE_CONNECTION_STRING / AUTOCLIP_HARNESS_STORAGE_CONTAINER so the harness can upload audio for Azure batch transcription.
- Optional overrides: AUTOCLIP_HARNESS_BATCH_ENDPOINT, \_BATCH_VERSION, \_BATCH_POLL_SECONDS, \_BATCH_TIMEOUT_MINUTES, and \_STORAGE_SAS_MINUTES.
- Station profiles are loaded from services/net/auto-clipper/Config/Stations by default; override with
  AUTOCLIP_HARNESS_STATION_PATH / AUTOCLIP_HARNESS_STATION.
- Outputs: clip_XX.\* media slices, clip_XX.txt transcripts, ranscript_full.txt, and
  llm_prompt_debug.txt (shows numbered transcript, heuristics, and the final prompt).

## Notes

- The harness shares the segmentation logic with the service, so any changes in ClipSegmentationService
  should be validated here first.
- Ensure ffmpeg is available on PATH; the harness shells out to ffmpeg to produce media clips.
