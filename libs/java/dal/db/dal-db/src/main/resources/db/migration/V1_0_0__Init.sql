CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Ensure the created audit columns are not changed.
-- Ensure the updated timestamp is updated.
CREATE OR REPLACE FUNCTION updateAudit()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'UPDATE') THEN
        NEW."createdOn" = OLD."createdOn";
        NEW."createdById" = OLD."createdById";
        NEW."createdBy" = OLD."createdBy";
    ELSIF (TG_OP = 'INSERT') THEN
        NEW."createdOn" = now();
    END IF;
    NEW."updatedOn" = now();
    RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';

CREATE TABLE IF NOT EXISTS public."User"
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
    CONSTRAINT "pk_User" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_User_name" ON public."User" ("username");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_User_key" ON public."User" ("key");
CREATE TRIGGER tr_auditUser BEFORE INSERT OR UPDATE ON public."User" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."DataSourceType"
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
    CONSTRAINT "pk_DataSourceType" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSourceType_name" ON public."DataSourceType" ("name");
CREATE TRIGGER tr_auditDataSourceType BEFORE INSERT OR UPDATE ON public."DataSourceType" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."License"
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
    CONSTRAINT "pk_License" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_License_name" ON public."License" ("name");
CREATE TRIGGER tr_auditLicense BEFORE INSERT OR UPDATE ON public."License" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."Schedule"
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
    CONSTRAINT "pk_Schedule" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Schedule_name" ON public."Schedule" ("name");
CREATE TRIGGER tr_auditSchedule BEFORE INSERT OR UPDATE ON public."Schedule" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."DataSource"
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
    "connection" JSON NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_DataSource" PRIMARY KEY ("id"),
    CONSTRAINT "fk_DataSourceType_DataSource" FOREIGN KEY ("typeId") REFERENCES public."DataSourceType" ("id"),
    CONSTRAINT "fk_License_DataSource" FOREIGN KEY ("licenseId") REFERENCES public."License" ("id"),
    CONSTRAINT "fk_Schedule_DataSource" FOREIGN KEY ("scheduleId") REFERENCES public."Schedule" ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSource_name" ON public."DataSource" ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSource_abbr" ON public."DataSource" ("abbr");
CREATE TRIGGER tr_auditDataSource BEFORE INSERT OR UPDATE ON public."DataSource" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."ContentReference"
(
    "source" VARCHAR(50) NOT NULL,
    "uid" VARCHAR(100) NOT NULL,
    "topic" VARCHAR(50) NOT NULL,
    "offset" INT NOT NULL DEFAULT -1,
    "status" INT NOT NULL DEFAULT 0,
    "publishedOn" TIMESTAMP WITH TIME ZONE,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_ContentReference" PRIMARY KEY ("source", "uid")
);
CREATE INDEX IF NOT EXISTS "idx_ContentReference_topic" ON public."ContentReference" ("topic");
CREATE TRIGGER tr_auditContentReference BEFORE INSERT OR UPDATE ON public."ContentReference" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

-------------------------------------------------------------------------------
-- Seed Data
-------------------------------------------------------------------------------

INSERT INTO public."DataSourceType" (
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

INSERT INTO public."License" (
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

INSERT INTO public."Schedule" (
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

INSERT INTO public."DataSource" (
    "name"
    , "abbr"
    , "typeId"
    , "licenseId"
    , "scheduleId"
    , "topic"
    , "connection"
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
    , '{ "url": "", "syndicationType": 0, "authenticationType": 0, "token": "" }' -- connection
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);