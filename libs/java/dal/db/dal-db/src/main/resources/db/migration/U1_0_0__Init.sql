DROP TABLE public."DataSource";
DROP TABLE public."Schedule";
DROP TABLE public."License";
DROP TABLE public."DataSourceType";
DROP TABLE public."User";

DELETE FROM public.flyway_schema_history
WHERE version = '1.0.0'