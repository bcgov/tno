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
    "firstName" VARCHAR(100),
    "lastName" VARCHAR(100),
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "emailVerified" BOOLEAN NOT NULL DEFAULT false,
    "lastLoginOn" TIMESTAMP WITH TIME ZONE,
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

CREATE TABLE IF NOT EXISTS public."Role"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "key" UUID,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Role" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Role_name" ON public."Role" ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Role_key" ON public."Role" ("key");
CREATE TRIGGER tr_auditRole BEFORE INSERT OR UPDATE ON public."Role" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."UserRole"
(
    "userId" INT NOT NULL,
    "roleId" INT NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_UserRole" PRIMARY KEY ("userId", "roleId"),
    CONSTRAINT "fk_User_UserRole" FOREIGN KEY ("userId") REFERENCES public."User" ("id"),
    CONSTRAINT "fk_Role_UserRole" FOREIGN KEY ("roleId") REFERENCES public."Role" ("id")
);
CREATE TRIGGER tr_auditUserRole BEFORE INSERT OR UPDATE ON public."UserRole" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."Claim"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "key" UUID,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Claim" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Claim_name" ON public."Claim" ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_Claim_key" ON public."Claim" ("key");
CREATE TRIGGER tr_auditClaim BEFORE INSERT OR UPDATE ON public."Claim" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."RoleClaim"
(
    "roleId" INT NOT NULL,
    "claimId" INT NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_RoleClaim" PRIMARY KEY ("roleId", "claimId"),
    CONSTRAINT "fk_Role_RoleClaim" FOREIGN KEY ("roleId") REFERENCES public."Role" ("id"),
    CONSTRAINT "fk_Claim_RoleClaim" FOREIGN KEY ("claimId") REFERENCES public."Claim" ("id")
);
CREATE TRIGGER tr_auditRoleClaim BEFORE INSERT OR UPDATE ON public."RoleClaim" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."DataSourceType"
(
    "id" INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000),
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
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
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
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
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
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
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
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

INSERT INTO public."Claim" (
    "name"
    , "key"
    , "isEnabled"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'administrator' -- name
    , 'db055cc6-b31c-42c4-99b5-5e519c31c8ab' -- key
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'editor' -- name
    , 'c374cbb1-7eda-4259-8f74-cd6c2287e32b' -- key
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'subscriber' -- name
    , '4818b135-034e-40d8-bd9c-8df2573ce9e0' -- key
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."Role" (
    "name"
    , "key"
    , "isEnabled"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'Administrator' -- name
    , 'c89a9f73-eaab-4c4f-8c90-f963de0c5854' -- key
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Editor' -- name
    , 'd5786a5b-4f71-46f1-9c0c-f6b69ee741b1' -- key
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Subscriber' -- name
    , '250eaa7f-67c9-4a60-b453-8ca790d569f5' -- key
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."User" (
    "username"
    , "key"
    , "email"
    , "displayName"
    , "emailVerified"
    , "isEnabled"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'admin' -- username
    , 'f3487a90-b793-4e60-b32c-2945591289aa' -- key
    , 'admin@local.com' -- email
    , 'Administrator' -- displayName
    , true -- emailVerified
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Editor' -- name
    , '1057697d-5cd0-4b00-97f3-df0f13849217' -- key
    , 'editor@local.com' -- email
    , 'Editor' -- displayName
    , true -- emailVerified
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Subscriber' -- name
    , 'e9b0ec48-c4c2-4c73-80a6-5c03da70261d' -- key
    , 'subscriber@local.com' -- email
    , 'Subscriber' -- displayName
    , true -- emailVerified
    , true -- isEnabled
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."UserRole" (
    "userId"
    , "roleId"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    '1' -- userId
    , '1' -- roleId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    '2' -- userId
    , '2' -- roleId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    '3' -- userId
    , '3' -- roleId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."RoleClaim" (
    "roleId"
    , "claimId"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    '1' -- roleId
    , '1' -- claimId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    '1' -- roleId
    , '2' -- claimId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    '1' -- roleId
    , '3' -- claimId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    '2' -- roleId
    , '2' -- claimId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    '3' -- roleId
    , '3' -- claimId
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);