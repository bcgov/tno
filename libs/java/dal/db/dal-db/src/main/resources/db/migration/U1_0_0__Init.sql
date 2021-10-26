DROP TABLE public."DataSource";
DROP TABLE public."Schedule";
DROP TABLE public."License";
DROP TABLE public."DataSourceType";
DROP TABLE public."DataSourceReference";
DROP TABLE public."UserRole";
DROP TABLE public."RoleClaim";
DROP TABLE public."User";
DROP TABLE public."Claim";

DELETE FROM public.flyway_schema_history
WHERE version = '1.0.0'