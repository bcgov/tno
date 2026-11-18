-- British Columbia stays on UTC-07:00 all year from 2026-11-01 (IANA tzdata 2026e, America/Vancouver).
-- 'Pacific Standard Time' (Windows id) and 'US/Pacific' both resolve to America/Los_Angeles, which still
-- falls back to UTC-08:00, so schedules, date conversions and Elasticsearch date math ran an hour off.
-- Only the quoted zone id is replaced; rows without it are untouched. Version is incremented so a stale
-- editor form can't save the old value back without a concurrency error.

-- .NET time zone ids: ingest and source schedules / date conversion.
UPDATE public."ingest"
SET "configuration" = replace("configuration"::text, '"Pacific Standard Time"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "configuration"::text LIKE '%"Pacific Standard Time"%';

UPDATE public."source"
SET "configuration" = replace("configuration"::text, '"Pacific Standard Time"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "configuration"::text LIKE '%"Pacific Standard Time"%';

-- .NET time zone ids: Razor templates (ReportExtensions.GetUtcOffset / TimeZoneInfo.FindSystemTimeZoneById).
UPDATE public."report_template"
SET "body" = replace("body", '"Pacific Standard Time"', '"America/Vancouver"'), "version" = "version" + 1
WHERE "body" LIKE '%"Pacific Standard Time"%';

UPDATE public."notification_template"
SET "body" = replace("body", '"Pacific Standard Time"', '"America/Vancouver"'), "version" = "version" + 1
WHERE "body" LIKE '%"Pacific Standard Time"%';

-- Elasticsearch "time_zone" in saved queries.
UPDATE public."filter"
SET "query" = replace("query"::text, '"US/Pacific"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "query"::text LIKE '%"US/Pacific"%';

UPDATE public."notification"
SET "query" = replace("query"::text, '"US/Pacific"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "query"::text LIKE '%"US/Pacific"%';

UPDATE public."report"
SET "settings" = replace("settings"::text, '"US/Pacific"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"US/Pacific"%';

UPDATE public."report_section"
SET "settings" = replace("settings"::text, '"US/Pacific"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"US/Pacific"%';

UPDATE public."folder"
SET "settings" = replace("settings"::text, '"US/Pacific"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"US/Pacific"%';

UPDATE public."event_schedule"
SET "settings" = replace("settings"::text, '"US/Pacific"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"US/Pacific"%';

UPDATE public."automation_profile"
SET "definition" = replace("definition"::text, '"US/Pacific"', '"America/Vancouver"')::jsonb, "version" = "version" + 1
WHERE "definition"::text LIKE '%"US/Pacific"%';
