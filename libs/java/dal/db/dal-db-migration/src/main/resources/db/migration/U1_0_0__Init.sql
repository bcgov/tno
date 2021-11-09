DROP TABLE public."DataSource";
DROP TABLE public."DataSourceType";
DROP TABLE public."Schedule";
DROP TABLE public."License";
DROP TABLE public."ContentReference";
DROP TABLE public."UserRole";
DROP TABLE public."RoleClaim";
DROP TABLE public."User";
DROP TABLE public."Role";
DROP TABLE public."Claim";

DROP SEQUENCE public.seq_DataSource;
DROP SEQUENCE public.seq_DataSourceType;
DROP SEQUENCE public.seq_Schedule;
DROP SEQUENCE public.seq_License;
DROP SEQUENCE public.seq_User;
DROP SEQUENCE public.seq_Role;
DROP SEQUENCE public.seq_Claim;

DELETE FROM public.flyway_schema_history
WHERE version = '1.0.0'