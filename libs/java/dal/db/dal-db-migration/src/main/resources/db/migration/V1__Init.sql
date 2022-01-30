CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Ensure the created audit columns are not changed.
-- Ensure the updated timestamp is updated.
CREATE OR REPLACE FUNCTION updateAudit()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'UPDATE') THEN
        NEW."created_on" = OLD."created_on";
        NEW."created_by_id" = OLD."created_by_id";
        NEW."created_by" = OLD."created_by";
    ELSIF (TG_OP = 'INSERT') THEN
        NEW."created_on" = now();
    END IF;
    NEW."updated_on" = now();
    RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';

CREATE SEQUENCE IF NOT EXISTS public.seq_user AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.user
(
    "id" INT NOT NULL DEFAULT nextval('seq_user'),
    "username" VARCHAR(50) NOT NULL,
    "email" VARCHAR(250) NOT NULL,
    "key" UUID NOT NULL DEFAULT uuid_generate_v4(),
    "display_name" VARCHAR(100) NOT NULL DEFAULT '',
    "first_name" VARCHAR(100) NOT NULL DEFAULT '',
    "last_name" VARCHAR(100) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "email_verified" BOOLEAN NOT NULL DEFAULT false,
    "last_login_on" TIMESTAMP WITH TIME ZONE,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_user" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_user_name" ON public.user ("username");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_user_key" ON public.user ("key");
CREATE TRIGGER tr_auditUser BEFORE INSERT OR UPDATE ON public.user FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_role AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.role
(
    "id" INT NOT NULL DEFAULT nextval('seq_role'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "key" UUID,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_role" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_role_name" ON public.role ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_role_key" ON public.role ("key");
CREATE TRIGGER tr_auditRole BEFORE INSERT OR UPDATE ON public.role FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.user_role
(
    "user_id" INT NOT NULL,
    "role_id" INT NOT NULL,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_user_role" PRIMARY KEY ("user_id", "role_id"),
    CONSTRAINT "fk_user_user_role" FOREIGN KEY ("user_id") REFERENCES public.user ("id"),
    CONSTRAINT "fk_role_user_role" FOREIGN KEY ("role_id") REFERENCES public.role ("id")
);
CREATE TRIGGER tr_auditUserRole BEFORE INSERT OR UPDATE ON public.user_role FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_claim AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.claim
(
    "id" INT NOT NULL DEFAULT nextval('seq_claim'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "key" UUID,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_claim" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_claim_name" ON public.claim ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_claim_key" ON public.claim ("key");
CREATE TRIGGER tr_auditClaim BEFORE INSERT OR UPDATE ON public.claim FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.role_claim
(
    "role_id" INT NOT NULL,
    "claim_id" INT NOT NULL,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_role_claim" PRIMARY KEY ("role_id", "claim_id"),
    CONSTRAINT "fk_role_role_claim" FOREIGN KEY ("role_id") REFERENCES public.role ("id"),
    CONSTRAINT "fk_claim_role_claim" FOREIGN KEY ("claim_id") REFERENCES public.claim ("id")
);
CREATE TRIGGER tr_auditRoleClaim BEFORE INSERT OR UPDATE ON public.role_claim FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_media_type AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.media_type
(
    "id" INT NOT NULL DEFAULT nextval('seq_media_type'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "sort_order" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_media_type" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_media_type_name" ON public.media_type ("name");
CREATE TRIGGER tr_auditMediaType BEFORE INSERT OR UPDATE ON public.media_type FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_license AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.license
(
    "id" INT NOT NULL DEFAULT nextval('seq_license'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "ttl" INT NOT NULL,
    "sort_order" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_license" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_license_name" ON public.license ("name");
CREATE TRIGGER tr_auditLicense BEFORE INSERT OR UPDATE ON public.license FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_schedule AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.schedule
(
    "id" INT NOT NULL DEFAULT nextval('seq_schedule'),
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "delay_ms" INT NOT NULL,
    "run_at" TIMESTAMP WITH TIME ZONE,
    "repeat" INT NOT NULL,
    "run_on_week_days" INT NOT NULL,
    "run_on_months" INT NOT NULL,
    "day_of_month" INT NOT NULL,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_schedule" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_schedule_name" ON public.schedule ("name");
CREATE TRIGGER tr_auditSchedule BEFORE INSERT OR UPDATE ON public.schedule FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_data_source AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.data_source
(
    "id" INT NOT NULL DEFAULT nextval('seq_data_source'),
    "name" VARCHAR(50) NOT NULL,
    "code" VARCHAR(10) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "media_type_id" INT NOT NULL,
    "license_id" INT NOT NULL,
    "schedule_id" INT NOT NULL,
    "topic" VARCHAR(50) NOT NULL,
    "last_ran_on" TIMESTAMP WITH TIME ZONE,
    "connection" TEXT NOT NULL,
    -- "connection" JSON NOT NULL, -- Hibernate has issues with JSON types.
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_data_source" PRIMARY KEY ("id"),
    CONSTRAINT "fk_media_type_data_source" FOREIGN KEY ("media_type_id") REFERENCES public.media_type ("id"),
    CONSTRAINT "fk_license_data_source" FOREIGN KEY ("license_id") REFERENCES public.license ("id"),
    CONSTRAINT "fk_schedule_data_source" FOREIGN KEY ("schedule_id") REFERENCES public.schedule ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_data_source_name" ON public.data_source ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_data_source_code" ON public.data_source ("code");
CREATE TRIGGER tr_auditDataSource BEFORE INSERT OR UPDATE ON public.data_source FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.content_reference
(
    "source" VARCHAR(50) NOT NULL,
    "uid" VARCHAR(100) NOT NULL,
    "topic" VARCHAR(50) NOT NULL,
    "partition" INT NOT NULL DEFAULT -1,
    "offset" BIGINT NOT NULL DEFAULT -1,
    "status" INT NOT NULL DEFAULT 0,
    "published_on" TIMESTAMP WITH TIME ZONE,
    "sourceUpdated_on" TIMESTAMP WITH TIME ZONE,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content_reference" PRIMARY KEY ("source", "uid")
);
CREATE INDEX IF NOT EXISTS "idx_content_reference_topic" ON public.content_reference ("topic");
CREATE INDEX IF NOT EXISTS "idx_content_reference_partition_offset" ON public.content_reference ("partition", "offset");
CREATE TRIGGER tr_auditContentReference BEFORE INSERT OR UPDATE ON public.content_reference FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_tone_pool AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.tone_pool
(
    "id" INT NOT NULL DEFAULT nextval('seq_tone_pool'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "owner_id" INT NOT NULL,
    "sort_order" INT NOT NULL DEFAULT 0,
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "is_public" BOOLEAN NOT NULL DEFAULT true,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_tone_pool" PRIMARY KEY ("id"),
    CONSTRAINT "fk_user_tone_pool" FOREIGN KEY ("owner_id") REFERENCES public.user ("id")
);
CREATE INDEX IF NOT EXISTS "idx_tone_pool_name" ON public.tone_pool ("owner_id", "name");
CREATE TRIGGER tr_auditTonePool BEFORE INSERT OR UPDATE ON public.tone_pool FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_category AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.category
(
    "id" INT NOT NULL DEFAULT nextval('seq_category'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "sort_order" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_category" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_category_name" ON public.category ("name");
CREATE TRIGGER tr_auditCategory BEFORE INSERT OR UPDATE ON public.category FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_action AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.action
(
    "id" INT NOT NULL DEFAULT nextval('seq_action'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "value_label" VARCHAR(100) NOT NULL DEFAULT '',
    "value_type" INT NOT NULL DEFAULT 0,
    "sort_order" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_action" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_action_name" ON public.action ("name");
CREATE TRIGGER tr_auditAction BEFORE INSERT OR UPDATE ON public.action FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.tag
(
    "id" VARCHAR(6) NOT NULL,
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "sort_order" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_tag" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_tag_name" ON public.tag ("name");
CREATE TRIGGER tr_auditTag BEFORE INSERT OR UPDATE ON public.tag FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_type AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content_type
(
    "id" INT NOT NULL DEFAULT nextval('seq_content_type'),
    "name" VARCHAR(100) NOT NULL,
    "description" VARCHAR(2000) NOT NULL DEFAULT '',
    "is_enabled" BOOLEAN NOT NULL DEFAULT true,
    "sort_order" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content_type" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_content_type_name" ON public.content_type ("name");
CREATE TRIGGER tr_auditContentType BEFORE INSERT OR UPDATE ON public.content_type FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content
(
    "id" INT NOT NULL DEFAULT nextval('seq_content'),
    "status" INT NOT NULL DEFAULT 0,
    "content_type_id" INT NOT NULL,
    "headline" VARCHAR(500) NOT NULL,
    "data_source_id" INT,
    "source" VARCHAR(100) NOT NULL DEFAULT '',
    "uid" VARCHAR(100) NOT NULL DEFAULT '',
    "license_id" INT NOT NULL,
    "media_type_id" INT NOT NULL,
    "page" VARCHAR(10) NOT NULL,
    "section" VARCHAR(100) NOT NULL,
    "published_on" TIMESTAMP WITH TIME ZONE,
    "report_id" INT,
    "summary" TEXT NOT NULL DEFAULT '',
    "source_url" VARCHAR(500) NOT NULL DEFAULT '',
    "transcription" TEXT NOT NULL DEFAULT '',
    "owner_id" INT NOT NULL,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content" PRIMARY KEY ("id"),
    CONSTRAINT "fk_user_content" FOREIGN KEY ("owner_id") REFERENCES public.user ("id"),
    CONSTRAINT "fk_media_type_content" FOREIGN KEY ("media_type_id") REFERENCES public.media_type ("id"),
    CONSTRAINT "fk_license_content" FOREIGN KEY ("license_id") REFERENCES public.license ("id"),
    CONSTRAINT "fk_data_source_content" FOREIGN KEY ("data_source_id") REFERENCES public.data_source ("id"),
    CONSTRAINT "fk_content_type_content" FOREIGN KEY ("content_type_id") REFERENCES public.content_type ("id")
);
CREATE INDEX IF NOT EXISTS "idx_content_type_headline" ON public.content ("headline");
CREATE INDEX IF NOT EXISTS "idx_content_type" ON public.content ("created_on", "published_on", "page", "section");
CREATE INDEX IF NOT EXISTS "idx_content_type_source_uid" ON public.content ("source", "uid");
CREATE TRIGGER tr_auditContent BEFORE INSERT OR UPDATE ON public.content FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.time_tracking
(
    "content_id" INT NOT NULL,
    "user_id" INT NOT NULL,
    "effort" DECIMAL NOT NULL DEFAULT 0,
    "activity" VARCHAR(100) NOT NULL,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_time_tracking" PRIMARY KEY ("content_id", "user_id"),
    CONSTRAINT "fk_content_time_tracking" FOREIGN KEY ("content_id") REFERENCES public.content ("id"),
    CONSTRAINT "fk_user_time_tracking" FOREIGN KEY ("user_id") REFERENCES public.user ("id")
);
CREATE TRIGGER tr_auditTimeTracking BEFORE INSERT OR UPDATE ON public.time_tracking FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.content_action
(
    "content_id" INT NOT NULL,
    "action_id" INT NOT NULL,
    "value" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content_action" PRIMARY KEY ("content_id", "action_id"),
    CONSTRAINT "fk_content_content_action" FOREIGN KEY ("content_id") REFERENCES public.content ("id"),
    CONSTRAINT "fk_action_content_action" FOREIGN KEY ("action_id") REFERENCES public.action ("id")
);
CREATE TRIGGER tr_auditContentAction BEFORE INSERT OR UPDATE ON public.content_action FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.content_tone
(
    "content_id" INT NOT NULL,
    "tone_pool_id" INT NOT NULL,
    "value" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content_tone" PRIMARY KEY ("content_id", "tone_pool_id"),
    CONSTRAINT "fk_content_content_tone" FOREIGN KEY ("content_id") REFERENCES public.content ("id"),
    CONSTRAINT "fk_tone_pool_content_tone" FOREIGN KEY ("tone_pool_id") REFERENCES public.tone_pool ("id")
);
CREATE TRIGGER tr_auditContentTone BEFORE INSERT OR UPDATE ON public.content_tone FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.content_category
(
    "content_id" INT NOT NULL,
    "category_id" INT NOT NULL,
    "score" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content_category" PRIMARY KEY ("content_id", "category_id"),
    CONSTRAINT "fk_content_content_category" FOREIGN KEY ("content_id") REFERENCES public.content ("id"),
    CONSTRAINT "fk_category_content_category" FOREIGN KEY ("category_id") REFERENCES public.category ("id")
);
CREATE TRIGGER tr_auditContentCategory BEFORE INSERT OR UPDATE ON public.content_category FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.content_tag
(
    "content_id" INT NOT NULL,
    "tag_id" VARCHAR(6) NOT NULL,
    "score" INT NOT NULL DEFAULT 0,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content_tag" PRIMARY KEY ("content_id", "tag_id"),
    CONSTRAINT "fk_content_content_tag" FOREIGN KEY ("content_id") REFERENCES public.content ("id"),
    CONSTRAINT "fk_tag_content_tag" FOREIGN KEY ("tag_id") REFERENCES public.tag ("id")
);
CREATE TRIGGER tr_auditContentTag BEFORE INSERT OR UPDATE ON public.content_tag FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE TABLE IF NOT EXISTS public.content_link
(
    "content_id" INT NOT NULL,
    "link_id" INT NOT NULL,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_content_link" PRIMARY KEY ("content_id", "link_id"),
    CONSTRAINT "fk_content_content_link" FOREIGN KEY ("content_id") REFERENCES public.content ("id"),
    CONSTRAINT "fk_content_content_link_Link" FOREIGN KEY ("link_id") REFERENCES public.content ("id")
);
CREATE TRIGGER tr_auditContentLink BEFORE INSERT OR UPDATE ON public.content_link FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_file_reference AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.file_reference
(
    "id" INT NOT NULL DEFAULT nextval('seq_file_reference'),
    "content_id" INT NOT NULL,
    "mime_type" VARCHAR(100) NOT NULL,
    "path" VARCHAR(500) NOT NULL,
    "size" INT NOT NULL,
    "length" INT NOT NULL,
    -- Audit Columns
    "created_by_id" UUID NOT NULL,
    "created_by" VARCHAR(50) NOT NULL,
    "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by_id" UUID NOT NULL,
    "updated_by" VARCHAR(50) NOT NULL,
    "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "pk_file_reference" PRIMARY KEY ("id"),
    CONSTRAINT "fk_content_file_reference" FOREIGN KEY ("content_id") REFERENCES public.content ("id")
);
CREATE TRIGGER tr_auditFileReference BEFORE INSERT OR UPDATE ON public.file_reference FOR EACH ROW EXECUTE PROCEDURE updateAudit();

-- Grant access to the admin user
DO
$do$
BEGIN
  IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE rolname = 'admin') THEN
      CREATE ROLE admin LOGIN;
   END IF;
END
$do$;
GRANT USAGE ON SCHEMA public TO admin;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO admin;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO admin;

-------------------------------------------------------------------------------
-- Seed Data
-------------------------------------------------------------------------------

INSERT INTO public.media_type (
    "name"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.license (
    "name"
    , "ttl"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.schedule (
    "name"
    , "delay_ms"
    , "repeat"
    , "run_on_week_days"
    , "run_on_months"
    , "day_of_month"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.claim (
    "name"
    , "key"
    , "is_enabled"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.role (
    "name"
    , "key"
    , "is_enabled"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.user (
    "username"
    , "key"
    , "email"
    , "display_name"
    , "email_verified"
    , "is_enabled"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.user_role (
    "user_id"
    , "role_id"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.role_claim (
    "role_id"
    , "claim_id"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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

INSERT INTO public.tone_pool (
    "name"
    , "owner_id"
    , "is_public"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
) VALUES (
    'Default'
    , 1
    , true
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public.action (
    "name"
    , "value_label"
    , "value_type"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
) VALUES (
    'Publish' -- name
    , '' -- value_label
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Alert' -- name
    , '' -- value_label
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Front Page' -- name
    , '' -- value_label
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Top Story' -- name
    , '' -- value_label
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'On Ticker' -- name
    , '' -- value_label
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Non Qualified Subject' -- name
    , '' -- value_label
    , 0 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
), (
    'Commentary' -- name
    , 'Commentary Timeout' -- value_label
    , 2 -- valueType
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public.category (
    "name"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
) VALUES (
    'TBD'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public.tag (
    "id"
    , "name"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
) VALUES (
    'TBD'
    , 'To Be Determined'
    , '00000000-0000-0000-0000-000000000000'
    , ''
    , '00000000-0000-0000-0000-000000000000'
    , ''
);

INSERT INTO public.content_type (
    "name"
    , "created_by_id"
    , "created_by"
    , "updated_by_id"
    , "updated_by"
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
