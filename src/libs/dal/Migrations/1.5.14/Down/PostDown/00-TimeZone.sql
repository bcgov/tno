-- Reverts 1.5.14. Every America/Vancouver id in these columns reverts, including any set after the migration ran.

-- .NET time zone ids: ingest and source schedules / date conversion.
UPDATE public."ingest"
SET "configuration" = replace("configuration"::text, '"America/Vancouver"', '"Pacific Standard Time"')::jsonb, "version" = "version" + 1
WHERE "configuration"::text LIKE '%"America/Vancouver"%';

UPDATE public."source"
SET "configuration" = replace("configuration"::text, '"America/Vancouver"', '"Pacific Standard Time"')::jsonb, "version" = "version" + 1
WHERE "configuration"::text LIKE '%"America/Vancouver"%';

-- .NET time zone ids: Razor templates (ReportExtensions.GetUtcOffset / TimeZoneInfo.FindSystemTimeZoneById).
UPDATE public."report_template"
SET "body" = replace("body", '"America/Vancouver"', '"Pacific Standard Time"'), "version" = "version" + 1
WHERE "body" LIKE '%"America/Vancouver"%';

UPDATE public."notification_template"
SET "body" = replace("body", '"America/Vancouver"', '"Pacific Standard Time"'), "version" = "version" + 1
WHERE "body" LIKE '%"America/Vancouver"%';

-- Elasticsearch "time_zone" in saved queries.
UPDATE public."filter"
SET "query" = replace("query"::text, '"America/Vancouver"', '"US/Pacific"')::jsonb, "version" = "version" + 1
WHERE "query"::text LIKE '%"America/Vancouver"%';

UPDATE public."notification"
SET "query" = replace("query"::text, '"America/Vancouver"', '"US/Pacific"')::jsonb, "version" = "version" + 1
WHERE "query"::text LIKE '%"America/Vancouver"%';

UPDATE public."report"
SET "settings" = replace("settings"::text, '"America/Vancouver"', '"US/Pacific"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"America/Vancouver"%';

UPDATE public."report_section"
SET "settings" = replace("settings"::text, '"America/Vancouver"', '"US/Pacific"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"America/Vancouver"%';

UPDATE public."folder"
SET "settings" = replace("settings"::text, '"America/Vancouver"', '"US/Pacific"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"America/Vancouver"%';

UPDATE public."event_schedule"
SET "settings" = replace("settings"::text, '"America/Vancouver"', '"US/Pacific"')::jsonb, "version" = "version" + 1
WHERE "settings"::text LIKE '%"America/Vancouver"%';

UPDATE public."automation_profile"
SET "definition" = replace("definition"::text, '"America/Vancouver"', '"US/Pacific"')::jsonb, "version" = "version" + 1
WHERE "definition"::text LIKE '%"America/Vancouver"%';
