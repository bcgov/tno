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

CREATE SEQUENCE IF NOT EXISTS public.seq_User AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."User"
(
    "id" INT NOT NULL DEFAULT nextval('seq_User'),
    "username" VARCHAR(50) NOT NULL,
    "email" VARCHAR(250) NOT NULL,
    "key" UUID NOT NULL DEFAULT uuid_generate_v4(),
    "displayName" VARCHAR(100) NOT NULL DEFAULT '',
    "firstName" VARCHAR(100) NOT NULL DEFAULT '',
    "lastName" VARCHAR(100) NOT NULL DEFAULT '',
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

CREATE SEQUENCE IF NOT EXISTS public.seq_Role AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."Role"
(
    "id" INT NOT NULL DEFAULT nextval('seq_Role'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
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

CREATE SEQUENCE IF NOT EXISTS public.seq_Claim AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."Claim"
(
    "id" INT NOT NULL DEFAULT nextval('seq_Claim'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
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

CREATE SEQUENCE IF NOT EXISTS public.seq_MediaType AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."MediaType"
(
    "id" INT NOT NULL DEFAULT nextval('seq_MediaType'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "sortOrder" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_MediaType" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_MediaType_name" ON public."MediaType" ("name");
CREATE TRIGGER tr_auditMediaType BEFORE INSERT OR UPDATE ON public."MediaType" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_License AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."License"
(
    "id" INT NOT NULL DEFAULT nextval('seq_License'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "ttl" INT NOT NULL,
    "sortOrder" INT NOT NULL DEFAULT 0,
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

CREATE SEQUENCE IF NOT EXISTS public.seq_Schedule AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."Schedule"
(
    "id" INT NOT NULL DEFAULT nextval('seq_Schedule'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
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

CREATE SEQUENCE IF NOT EXISTS public.seq_DataSource AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."DataSource"
(
    "id" INT NOT NULL DEFAULT nextval('seq_DataSource'),
    "name" VARCHAR(50) NOT NULL,
    "code" VARCHAR(10) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "mediaTypeId" INT NOT NULL,
    "licenseId" INT NOT NULL,
    "scheduleId" INT NOT NULL,
    "topic" VARCHAR(50) NOT NULL,
    "lastRanOn" TIMESTAMP WITH TIME ZONE,
    "connection" TEXT NOT NULL,
    -- "connection" JSON NOT NULL, -- Hibernate has issues with JSON types.
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_DataSource" PRIMARY KEY ("id"),
    CONSTRAINT "fk_MediaType_DataSource" FOREIGN KEY ("mediaTypeId") REFERENCES public."MediaType" ("id"),
    CONSTRAINT "fk_License_DataSource" FOREIGN KEY ("licenseId") REFERENCES public."License" ("id"),
    CONSTRAINT "fk_Schedule_DataSource" FOREIGN KEY ("scheduleId") REFERENCES public."Schedule" ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSource_name" ON public."DataSource" ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_DataSource_code" ON public."DataSource" ("code");
CREATE TRIGGER tr_auditDataSource BEFORE INSERT OR UPDATE ON public."DataSource" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."ContentReference"
(
    "source" VARCHAR(50) NOT NULL,
    "uid" VARCHAR(100) NOT NULL,
    "topic" VARCHAR(50) NOT NULL,
    "partition" INT NOT NULL DEFAULT -1,
    "offset" BIGINT NOT NULL DEFAULT -1,
    "status" INT NOT NULL DEFAULT 0,
    "publishedOn" TIMESTAMP WITH TIME ZONE,
    "sourceUpdatedOn" TIMESTAMP WITH TIME ZONE,
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
CREATE INDEX IF NOT EXISTS "idx_ContentReference_partition_offset" ON public."ContentReference" ("partition", "offset");
CREATE TRIGGER tr_auditContentReference BEFORE INSERT OR UPDATE ON public."ContentReference" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_TonePool AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."TonePool"
(
    "id" INT NOT NULL DEFAULT nextval('seq_TonePool'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "ownerId" INT NOT NULL,
    "sortOrder" INT NOT NULL DEFAULT 0,
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "isPublic" BOOLEAN NOT NULL DEFAULT true,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_TonePool" PRIMARY KEY ("id"),
    CONSTRAINT "fk_User_TonePool" FOREIGN KEY ("ownerId") REFERENCES public."User" ("id")
);
CREATE INDEX IF NOT EXISTS "idx_TonePool_name" ON public."TonePool" ("ownerId", "name");
CREATE TRIGGER tr_auditTonePool BEFORE INSERT OR UPDATE ON public."TonePool" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_Category AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."Category"
(
    "id" INT NOT NULL DEFAULT nextval('seq_Category'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "sortOrder" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Category" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_Category_name" ON public."Category" ("name");
CREATE TRIGGER tr_auditCategory BEFORE INSERT OR UPDATE ON public."Category" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_Action AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."Action"
(
    "id" INT NOT NULL DEFAULT nextval('seq_Action'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "valueLabel" VARCHAR(100) NOT NULL DEFAULT '',
    "valueType" INT NOT NULL DEFAULT 0,
    "sortOrder" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Action" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_Action_name" ON public."Action" ("name");
CREATE TRIGGER tr_auditAction BEFORE INSERT OR UPDATE ON public."Action" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."Tag"
(
    "id" VARCHAR(6) NOT NULL,
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "sortOrder" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Tag" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_Tag_name" ON public."Tag" ("name");
CREATE TRIGGER tr_auditTag BEFORE INSERT OR UPDATE ON public."Tag" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_ContentType AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."ContentType"
(
    "id" INT NOT NULL DEFAULT nextval('seq_ContentType'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "isEnabled" BOOLEAN NOT NULL DEFAULT true,
    "sortOrder" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_ContentType" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_ContentType_name" ON public."ContentType" ("name");
CREATE TRIGGER tr_auditContentType BEFORE INSERT OR UPDATE ON public."ContentType" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_Content AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."Content"
(
    "id" INT NOT NULL DEFAULT nextval('seq_Content'),
    "status" INT NOT NULL DEFAULT 0,
    "contentTypeId" INT NOT NULL,
    "headline" VARCHAR(500) NOT NULL,
    "dataSourceId" INT,
    "source" VARCHAR(100) NOT NULL DEFAULT '',
    "uid" VARCHAR(100) NOT NULL DEFAULT '',
    "licenseId" INT NOT NULL,
    "mediaTypeId" INT NOT NULL,
    "page" VARCHAR(10) NOT NULL,
    "section" VARCHAR(100) NOT NULL,
    "publishedOn" TIMESTAMP WITH TIME ZONE,
    "reportId" INT,
    "summary" TEXT NOT NULL DEFAULT '',
    "sourceURL" VARCHAR(500) NOT NULL DEFAULT '',
    "transcription" TEXT NOT NULL DEFAULT '',
    "ownerId" INT NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_Content" PRIMARY KEY ("id"),
    CONSTRAINT "fk_User_Content" FOREIGN KEY ("ownerId") REFERENCES public."User" ("id"),
    CONSTRAINT "fk_MediaType_Content" FOREIGN KEY ("mediaTypeId") REFERENCES public."MediaType" ("id"),
    CONSTRAINT "fk_License_Content" FOREIGN KEY ("licenseId") REFERENCES public."License" ("id"),
    CONSTRAINT "fk_ContentType_Content" FOREIGN KEY ("contentTypeId") REFERENCES public."ContentType" ("id")
);
CREATE INDEX IF NOT EXISTS "idx_ContentType_headline" ON public."Content" ("headline");
CREATE INDEX IF NOT EXISTS "idx_ContentType" ON public."Content" ("createdOn", "publishedOn", "page", "section");
CREATE INDEX IF NOT EXISTS "idx_ContentType_source_uid" ON public."Content" ("source", "uid");
CREATE TRIGGER tr_auditContent BEFORE INSERT OR UPDATE ON public."Content" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."TimeTracking"
(
    "contentId" INT NOT NULL,
    "userId" INT NOT NULL,
    "effort" DECIMAL NOT NULL DEFAULT 0,
    "activity" VARCHAR(100) NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_TimeTracking" PRIMARY KEY ("contentId", "userId"),
    CONSTRAINT "fk_Content_TimeTracking" FOREIGN KEY ("contentId") REFERENCES public."Content" ("id"),
    CONSTRAINT "fk_User_TimeTracking" FOREIGN KEY ("userId") REFERENCES public."User" ("id")
);
CREATE TRIGGER tr_auditTimeTracking BEFORE INSERT OR UPDATE ON public."TimeTracking" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."ContentAction"
(
    "contentId" INT NOT NULL,
    "actionId" INT NOT NULL,
    "value" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_ContentAction" PRIMARY KEY ("contentId", "actionId"),
    CONSTRAINT "fk_Content_ContentAction" FOREIGN KEY ("contentId") REFERENCES public."Content" ("id"),
    CONSTRAINT "fk_Action_ContentAction" FOREIGN KEY ("actionId") REFERENCES public."Action" ("id")
);
CREATE TRIGGER tr_auditContentAction BEFORE INSERT OR UPDATE ON public."ContentAction" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."ContentTone"
(
    "contentId" INT NOT NULL,
    "tonePoolId" INT NOT NULL,
    "value" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_ContentTone" PRIMARY KEY ("contentId", "tonePoolId"),
    CONSTRAINT "fk_Content_ContentTone" FOREIGN KEY ("contentId") REFERENCES public."Content" ("id"),
    CONSTRAINT "fk_TonePool_ContentTone" FOREIGN KEY ("tonePoolId") REFERENCES public."TonePool" ("id")
);
CREATE TRIGGER tr_auditContentTone BEFORE INSERT OR UPDATE ON public."ContentTone" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."ContentCategory"
(
    "contentId" INT NOT NULL,
    "categoryId" INT NOT NULL,
    "score" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_ContentCategory" PRIMARY KEY ("contentId", "categoryId"),
    CONSTRAINT "fk_Content_ContentCategory" FOREIGN KEY ("contentId") REFERENCES public."Content" ("id"),
    CONSTRAINT "fk_Category_ContentCategory" FOREIGN KEY ("categoryId") REFERENCES public."Category" ("id")
);
CREATE TRIGGER tr_auditContentCategory BEFORE INSERT OR UPDATE ON public."ContentCategory" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."ContentTag"
(
    "contentId" INT NOT NULL,
    "tagId" VARCHAR(6) NOT NULL,
    "score" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_ContentTag" PRIMARY KEY ("contentId", "tagId"),
    CONSTRAINT "fk_Content_ContentTag" FOREIGN KEY ("contentId") REFERENCES public."Content" ("id"),
    CONSTRAINT "fk_Tag_ContentTag" FOREIGN KEY ("tagId") REFERENCES public."Tag" ("id")
);
CREATE TRIGGER tr_auditContentTag BEFORE INSERT OR UPDATE ON public."ContentTag" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public."ContentLink"
(
    "contentId" INT NOT NULL,
    "linkId" INT NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_ContentLink" PRIMARY KEY ("contentId", "linkId"),
    CONSTRAINT "fk_Content_ContentLink" FOREIGN KEY ("contentId") REFERENCES public."Content" ("id"),
    CONSTRAINT "fk_Content_ContentLink_Link" FOREIGN KEY ("linkId") REFERENCES public."Content" ("id")
);
CREATE TRIGGER tr_auditContentLink BEFORE INSERT OR UPDATE ON public."ContentLink" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_FileReference AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public."FileReference"
(
    "id" INT NOT NULL DEFAULT nextval('seq_FileReference'),
    "contentId" INT NOT NULL,
    "mimeType" VARCHAR(100) NOT NULL,
    "path" VARCHAR(500) NOT NULL,
    "size" INT NOT NULL,
    "length" INT NOT NULL,
    -- Audit Columns
    "createdById" UUID NOT NULL,
    "createdBy" VARCHAR(50) NOT NULL,
    "createdOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedById" UUID NOT NULL,
    "updatedBy" VARCHAR(50) NOT NULL,
    "updatedOn" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_FileReference" PRIMARY KEY ("id"),
    CONSTRAINT "fk_Content_FileReference" FOREIGN KEY ("contentId") REFERENCES public."Content" ("id")
);
CREATE TRIGGER tr_auditFileReference BEFORE INSERT OR UPDATE ON public."FileReference" FOR EACH ROW EXECUTE PROCEDURE updateAudit();

-------------------------------------------------------------------------------
-- Seed Data
-------------------------------------------------------------------------------

INSERT INTO public."MediaType" (
    "name"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'ATOM'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'RSS'
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
    'Regular Expire'
    , 90 -- ttl
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Special Expire'
    , 150 -- ttl
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Never Expire'
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

INSERT INTO public."TonePool" (
    "name"
    , "ownerId"
    , "isPublic"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'Default'
    , 1
    , true
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."Action" (
    "name"
    , "valueLabel"
    , "valueType"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'Publish' -- name
    , '' -- valueLabel
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Alert' -- name
    , '' -- valueLabel
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Front Page' -- name
    , '' -- valueLabel
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Top Story' -- name
    , '' -- valueLabel
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'On Ticker' -- name
    , '' -- valueLabel
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Non Qualified Subject' -- name
    , '' -- valueLabel
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Commentary' -- name
    , 'Commentary Timeout' -- valueLabel
    , 2 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."Category" (
    "name"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'TBD'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."Tag" (
    "id"
    , "name"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'TBD'
    , 'To Be Determined'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public."ContentType" (
    "name"
    , "createdById"
    , "createdBy"
    , "updatedById"
    , "updatedBy"
) VALUES (
    'Snippet'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Syndication'
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
), (
    'TV'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);