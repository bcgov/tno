DROP TABLE public."SyndicationSources";
DROP TABLE public."DataSources";
DROP TABLE public."Schedules";
DROP TABLE public."Licenses";
DROP TABLE public."DataSourceTypes";

DELETE FROM public.flyway_schema_history
WHERE version = '1.0.0'