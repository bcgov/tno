CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE TABLE IF NOT EXISTS public."Users"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "username" VARCHAR(50) NOT NULL,
    "email" VARCHAR(250) NOT NULL,
    "key" UUID NOT NULL DEFAULT uuid_generate_v4(),
    "displayName" VARCHAR(100),
    "isEnabled" BOOLEAN DEFAULT true,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Users" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Users_name" ON public."Users" ("username");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Users_key" ON public."Users" ("key");

CREATE TABLE IF NOT EXISTS public."DataSourceTypes"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN DEFAULT true,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_DataSourceTypes" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSourceTypes_name" ON public."DataSourceTypes" ("name");

CREATE TABLE IF NOT EXISTS public."Licenses"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN DEFAULT true,
    "ttl" INT NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Licenses" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Licenses_name" ON public."Licenses" ("name");

CREATE TABLE IF NOT EXISTS public."Schedules"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN DEFAULT true,
    "delayMS" INT NOT NULL,
    "runAt" TIMESTAMP WITH TIME ZONE,
    "repeat" INT NOT NULL,
    "runOnWeekDays" INT NOT NULL,
    "runOnMonths" INT NOT NULL,
    "dayOfMonth" INT NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Schedules" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Schedules_name" ON public."Schedules" ("name");

CREATE TABLE IF NOT EXISTS public."DataSources"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "abbr" VARCHAR(10) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN DEFAULT true,
    "typeId" INT NOT NULL,
    "licenseId" INT NOT NULL,
    "scheduleId" INT NOT NULL,
    "topic" VARCHAR(50) NOT NULL,
    "lastRanOn" TIMESTAMP WITH TIME ZONE,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_DataSources" PRIMARY KEY ("id"),
    CONSTRAINT "fk_DataSourceTypes_DataSources" FOREIGN KEY ("typeId") REFERENCES public."DataSourceTypes" ("id"),
    CONSTRAINT "fk_Licenses_DataSources" FOREIGN KEY ("licenseId") REFERENCES public."Licenses" ("id"),
    CONSTRAINT "fk_Schedules_DataSources" FOREIGN KEY ("scheduleId") REFERENCES public."Schedules" ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSources_name" ON public."DataSources" ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSources_abbr" ON public."DataSources" ("abbr");

CREATE TABLE IF NOT EXISTS public."SyndicationSources"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN DEFAULT true,
    "url" VARCHAR(500) NOT NULL,
    "typeId" INT NOT NULL,
    "dataSourceId" INT NOT NULL,
    "topic" VARCHAR(50) NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_SyndicationSources" PRIMARY KEY ("id"),
    CONSTRAINT "fk_DataSources_SyndicationSources" FOREIGN KEY ("dataSourceId") REFERENCES public."DataSources" ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_SyndicationSources_name" ON public."SyndicationSources" ("name");

-------------------------------------------------------------------------------
-- Seed Data
-------------------------------------------------------------------------------

INSERT INTO public."DataSourceTypes" (
    "name"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'Syndication'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'TV'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Radio'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."Licenses" (
    "name"
    , "ttl"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'Forever'
    , 0 -- ttl
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."Schedules" (
    "name"
    , "delayMS"
    , "repeat"
    , "runOnWeekDays"
    , "runOnMonths"
    , "dayOfMonth"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'Continous - 30s'
    , 30 -- delayMS
    , 0 -- repeat
    , 0 -- runOnWeekDays
    , 0 -- runOnMonths
    , 0 -- dayOfMonth
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."DataSources" (
    "name"
    , "abbr"
    , "typeId"
    , "licenseId"
    , "scheduleId"
    , "topic"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'National Post - RSS'
    , 'NTLP'
    , 1 -- typeId
    , 1 -- licenseId
    , 1 -- scheduleId
    , '' -- topic
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);