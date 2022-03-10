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
CREATE SEQUENCE IF NOT EXISTS public.seq_user_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_user_version'),
  CONSTRAINT "pk_user" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_user_name" ON public.user ("username");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_user_key" ON public.user ("key");
CREATE TRIGGER tr_audit_user BEFORE INSERT OR UPDATE ON public.user FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_role AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_role_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_role_version'),
  CONSTRAINT "pk_role" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_role_name" ON public.role ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_role_key" ON public.role ("key");
CREATE TRIGGER tr_audit_role BEFORE INSERT OR UPDATE ON public.role FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_user_role_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_user_role_version'),
  CONSTRAINT "pk_user_role" PRIMARY KEY ("user_id", "role_id"),
  CONSTRAINT "fk_user_user_role" FOREIGN KEY ("user_id") REFERENCES public.user ("id"),
  CONSTRAINT "fk_role_user_role" FOREIGN KEY ("role_id") REFERENCES public.role ("id")
);
CREATE TRIGGER tr_audit_user_role BEFORE INSERT OR UPDATE ON public.user_role FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_claim AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_claim_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_claim_version'),
  CONSTRAINT "pk_claim" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_claim_name" ON public.claim ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_claim_key" ON public.claim ("key");
CREATE TRIGGER tr_audit_claim BEFORE INSERT OR UPDATE ON public.claim FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_role_claim_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_role_claim_version'),
  CONSTRAINT "pk_role_claim" PRIMARY KEY ("role_id", "claim_id"),
  CONSTRAINT "fk_role_role_claim" FOREIGN KEY ("role_id") REFERENCES public.role ("id"),
  CONSTRAINT "fk_claim_role_claim" FOREIGN KEY ("claim_id") REFERENCES public.claim ("id")
);
CREATE TRIGGER tr_audit_role_claim BEFORE INSERT OR UPDATE ON public.role_claim FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_media_type AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_media_type_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_media_type_version'),
  CONSTRAINT "pk_media_type" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_media_type_name" ON public.media_type ("name");
CREATE TRIGGER tr_audit_media_type BEFORE INSERT OR UPDATE ON public.media_type FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_license AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_license_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_license_version'),
  CONSTRAINT "pk_license" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_license_name" ON public.license ("name");
CREATE TRIGGER tr_audit_license BEFORE INSERT OR UPDATE ON public.license FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_schedule AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_schedule_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.schedule
(
  "id" INT NOT NULL DEFAULT nextval('seq_schedule'),
  "name" VARCHAR(50) NOT NULL,
  "description" VARCHAR(2000) NOT NULL DEFAULT '',
  "is_enabled" BOOLEAN NOT NULL DEFAULT true,
  "schedule_type" INT NOT NULL DEFAULT 0,
  "delay_ms" INT NOT NULL,
  "run_on" TIMESTAMP WITH TIME ZONE,
  "start_at" TIME,
  "stop_at" TIME,
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_schedule_version'),
  CONSTRAINT "pk_schedule" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_schedule_name" ON public.schedule ("name");
CREATE TRIGGER tr_audit_schedule BEFORE INSERT OR UPDATE ON public.schedule FOR EACH ROW EXECUTE PROCEDURE updateAudit();


CREATE SEQUENCE IF NOT EXISTS public.seq_data_location AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_data_location_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.data_location
(
  "id" INT NOT NULL DEFAULT nextval('seq_data_location'),
  "name" VARCHAR(50) NOT NULL,
  "description" VARCHAR(2000) NOT NULL DEFAULT '',
  "is_enabled" BOOLEAN NOT NULL DEFAULT true,
  "sort_order" INT NOT NULL DEFAULT 0,
  -- "connection" JSON NOT NULL, -- Hibernate has issues with JSON types.
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_data_location_version'),
  CONSTRAINT "pk_data_location" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_data_location_name" ON public.data_location ("name");
CREATE TRIGGER tr_audit_data_location BEFORE INSERT OR UPDATE ON public.data_location FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_data_source AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_data_source_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.data_source
(
  "id" INT NOT NULL DEFAULT nextval('seq_data_source'),
  "name" VARCHAR(100) NOT NULL,
  "short_name" VARCHAR(50) NOT NULL DEFAULT '',
  "code" VARCHAR(20) NOT NULL,
  "description" VARCHAR(2000) NOT NULL DEFAULT '',
  "is_enabled" BOOLEAN NOT NULL DEFAULT true,
  "data_location_id" INT NOT NULL,
  "media_type_id" INT NOT NULL,
  "license_id" INT NOT NULL,
  "topic" VARCHAR(50) NOT NULL,
  "last_ran_on" TIMESTAMP WITH TIME ZONE,
  "retry_limit" INT NOT NULL DEFAULT 3,
  "failed_attempts" INT NOT NULL DEFAULT 0,
  "in_cbra" BOOLEAN NOT NULL DEFAULT false,
  "in_analysis" BOOLEAN NOT NULL DEFAULT false,
  "connection" TEXT NOT NULL,
  "parent_id" INT,
  -- "connection" JSON NOT NULL, -- Hibernate has issues with JSON types.
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_data_source_version'),
  CONSTRAINT "pk_data_source" PRIMARY KEY ("id"),
  CONSTRAINT "fk_location_data_source" FOREIGN KEY ("data_location_id") REFERENCES public.data_location ("id"),
  CONSTRAINT "fk_media_type_data_source" FOREIGN KEY ("media_type_id") REFERENCES public.media_type ("id"),
  CONSTRAINT "fk_license_data_source" FOREIGN KEY ("license_id") REFERENCES public.license ("id"),
  CONSTRAINT "fk_data_source_data_source" FOREIGN KEY ("parent_id") REFERENCES public.data_source ("id")
);
CREATE UNIQUE INDEX IF NOT EXISTS "idx_data_source_name" ON public.data_source ("name");
CREATE UNIQUE INDEX IF NOT EXISTS "idx_data_source_code" ON public.data_source ("code");
CREATE TRIGGER tr_audit_data_source BEFORE INSERT OR UPDATE ON public.data_source FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_data_source_schedule_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.data_source_schedule
(
  "data_source_id" INT NOT NULL,
  "schedule_id" INT NOT NULL,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_data_source_schedule_version'),
  CONSTRAINT "pk_data_source_schedule" PRIMARY KEY ("data_source_id", "schedule_id"),
  CONSTRAINT "fk_data_source_data_source_schedule" FOREIGN KEY ("data_source_id") REFERENCES public.data_source ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_schedule_data_source_schedule" FOREIGN KEY ("schedule_id") REFERENCES public.schedule ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_data_source_schedule BEFORE INSERT OR UPDATE ON public.data_source_schedule FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_reference_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content_reference
(
  "source" VARCHAR(50) NOT NULL,
  "uid" VARCHAR(100) NOT NULL,
  "topic" VARCHAR(50) NOT NULL,
  "partition" INT NOT NULL DEFAULT -1,
  "offset" BIGINT NOT NULL DEFAULT -1,
  "status" INT NOT NULL DEFAULT 0,
  "published_on" TIMESTAMP WITH TIME ZONE,
  "source_updated_on" TIMESTAMP WITH TIME ZONE,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_reference_version'),
  CONSTRAINT "pk_content_reference" PRIMARY KEY ("source", "uid")
);
CREATE INDEX IF NOT EXISTS "idx_content_reference_topic" ON public.content_reference ("topic");
CREATE INDEX IF NOT EXISTS "idx_content_reference_partition_offset" ON public.content_reference ("partition", "offset");
CREATE TRIGGER tr_audit_content_reference BEFORE INSERT OR UPDATE ON public.content_reference FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_reference_log AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content_reference_log
(
  "id" INT NOT NULL DEFAULT nextval('seq_content_reference_log'),
  "source" VARCHAR(50) NOT NULL,
  "uid" VARCHAR(100) NOT NULL,
  "status" INT NOT NULL,
  "message" VARCHAR(500) NOT NULL,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL,
  CONSTRAINT "pk_content_reference_log" PRIMARY KEY ("id"),
  CONSTRAINT "fk_content_reference_content_reference_log" FOREIGN KEY ("source", "uid") REFERENCES public.content_reference ("source", "uid") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "idx_content_reference_log_created_on" ON public.content_reference_log ("created_on");
CREATE TRIGGER tr_audit_content_reference_log BEFORE INSERT OR UPDATE ON public.content_reference_log FOR EACH ROW EXECUTE PROCEDURE updateAudit();

-- Keeps track of changes to content references.
CREATE OR REPLACE FUNCTION updateContentReferenceLog()
RETURNS TRIGGER AS $$
BEGIN
  IF (TG_OP = 'UPDATE' AND OLD."status" != NEW."status") THEN
    INSERT INTO public.content_reference_log (
      "source"
      , "uid"
      , "status"
      , "message"
      , "created_by_id"
      , "created_by"
      , "created_on"
      , "updated_by_id"
      , "updated_by"
      , "updated_on"
      , "version"
    ) VALUES (
      NEW."source"
      , NEW."uid"
      , NEW."status"
      , 'status: [' || CAST(OLD."status" AS VARCHAR) || ':' || CAST(NEW."status" AS VARCHAR) || ']'
      , NEW."created_by_id"
      , NEW."created_by"
      , now()
      , OLD."created_by_id"
      , OLD."created_by"
      , now()
      , OLD."version"
    );
  ELSIF (TG_OP = 'INSERT') THEN
    INSERT INTO public.content_reference_log (
      "source"
      , "uid"
      , "status"
      , "message"
      , "created_by_id"
      , "created_by"
      , "created_on"
      , "updated_by_id"
      , "updated_by"
      , "updated_on"
      , "version"
    ) VALUES (
      NEW."source"
      , NEW."uid"
      , NEW."status"
      , 'Added'
      , NEW."created_by_id"
      , NEW."created_by"
      , now()
      , NEW."created_by_id"
      , NEW."created_by"
      , now()
      , NEW."version"
    );
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';
CREATE TRIGGER tr_log_content_reference AFTER INSERT OR UPDATE ON public.content_reference FOR EACH ROW EXECUTE PROCEDURE updateContentReferenceLog();

CREATE SEQUENCE IF NOT EXISTS public.seq_tone_pool AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_tone_pool_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_tone_pool_version'),
  CONSTRAINT "pk_tone_pool" PRIMARY KEY ("id"),
  CONSTRAINT "fk_user_tone_pool" FOREIGN KEY ("owner_id") REFERENCES public.user ("id")
);
CREATE INDEX IF NOT EXISTS "idx_tone_pool_name" ON public.tone_pool ("owner_id", "name");
CREATE TRIGGER tr_audit_tone_pool BEFORE INSERT OR UPDATE ON public.tone_pool FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_category AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_category_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_category_version'),
  CONSTRAINT "pk_category" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_category_name" ON public.category ("name");
CREATE TRIGGER tr_audit_category BEFORE INSERT OR UPDATE ON public.category FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_action AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_action_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_action_version'),
  CONSTRAINT "pk_action" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_action_name" ON public.action ("name");
CREATE TRIGGER tr_audit_action BEFORE INSERT OR UPDATE ON public.action FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_tag_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_tag_version'),
  CONSTRAINT "pk_tag" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_tag_name" ON public.tag ("name");
CREATE TRIGGER tr_audit_tag BEFORE INSERT OR UPDATE ON public.tag FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_series AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_series_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.series
(
  "id" INT NOT NULL DEFAULT nextval('seq_series'),
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_series_version'),
  CONSTRAINT "pk_series" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_series_name" ON public.series ("name");
CREATE TRIGGER tr_audit_series BEFORE INSERT OR UPDATE ON public.series FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_type AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_content_type_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_type_version'),
  CONSTRAINT "pk_content_type" PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS "idx_content_type_name" ON public.content_type ("name");
CREATE TRIGGER tr_audit_content_type BEFORE INSERT OR UPDATE ON public.content_type FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_type_action_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content_type_action
(
  "content_type_id" INT NOT NULL,
  "action_id" INT NOT NULL,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_type_action_version'),
  CONSTRAINT "pk_content_type_action" PRIMARY KEY ("content_type_id", "action_id"),
  CONSTRAINT "fk_content_type_content_type_action" FOREIGN KEY ("content_type_id") REFERENCES public.content_type ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_action_content_type_action" FOREIGN KEY ("action_id") REFERENCES public.action ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_content_type_action BEFORE INSERT OR UPDATE ON public.content_type_action FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_content_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content
(
  "id" INT NOT NULL DEFAULT nextval('seq_content'),
  "status" INT NOT NULL DEFAULT 0,
  "workflow_status" INT NOT NULL DEFAULT 0,
  "content_type_id" INT NOT NULL,
  "media_type_id" INT NOT NULL,
  "license_id" INT NOT NULL,
  "series_id" INT,
  "data_source_id" INT,
  "source" VARCHAR(100) NOT NULL DEFAULT '',
  "uid" VARCHAR(100) NOT NULL DEFAULT '',
  "headline" VARCHAR(500) NOT NULL,
  "page" VARCHAR(10) NOT NULL,
  "published_on" TIMESTAMP WITH TIME ZONE,
  "summary" TEXT NOT NULL DEFAULT '',
  "source_url" VARCHAR(500) NOT NULL DEFAULT '',
  "transcription" TEXT NOT NULL DEFAULT '',
  "owner_id" INT,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_version'),
  CONSTRAINT "pk_content" PRIMARY KEY ("id"),
  CONSTRAINT "fk_content_type_content" FOREIGN KEY ("content_type_id") REFERENCES public.content_type ("id"),
  CONSTRAINT "fk_media_type_content" FOREIGN KEY ("media_type_id") REFERENCES public.media_type ("id"),
  CONSTRAINT "fk_user_content" FOREIGN KEY ("owner_id") REFERENCES public.user ("id"),
  CONSTRAINT "fk_license_content" FOREIGN KEY ("license_id") REFERENCES public.license ("id"),
  CONSTRAINT "fk_data_source_content" FOREIGN KEY ("data_source_id") REFERENCES public.data_source ("id"),
  CONSTRAINT "fk_series_content" FOREIGN KEY ("series_id") REFERENCES public.series ("id")
);
CREATE INDEX IF NOT EXISTS "idx_content_headline" ON public.content ("headline", "created_by", "updated_by");
CREATE INDEX IF NOT EXISTS "idx_content" ON public.content ("created_on", "published_on", "page");
CREATE INDEX IF NOT EXISTS "idx_content_source_uid" ON public.content ("source", "uid");
CREATE TRIGGER tr_audit_content BEFORE INSERT OR UPDATE ON public.content FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_log AS INT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content_log
(
  "id" INT NOT NULL DEFAULT nextval('seq_content_log'),
  "content_id" INT NOT NULL,
  "status" INT NOT NULL,
  "workflow_status" INT NOT NULL,
  "message" VARCHAR(500) NOT NULL,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL,
  CONSTRAINT "pk_content_log" PRIMARY KEY ("id"),
  CONSTRAINT "fk_content_content_log" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "idx_content_log_created_on" ON public.content_log ("created_on", "created_by", "updated_by");
CREATE TRIGGER tr_audit_content_log BEFORE INSERT OR UPDATE ON public.content_log FOR EACH ROW EXECUTE PROCEDURE updateAudit();

-- Keeps track of changes to content.
CREATE OR REPLACE FUNCTION updateContentLog()
RETURNS TRIGGER AS $$
BEGIN
  IF (TG_OP = 'UPDATE' AND (OLD."status" != NEW."status" OR OLD."workflow_status" != NEW."workflow_status")) THEN
    INSERT INTO public.content_log (
      "content_id"
      , "status"
      , "workflow_status"
      , "message"
      , "created_by_id"
      , "created_by"
      , "created_on"
      , "updated_by_id"
      , "updated_by"
      , "updated_on"
      , "version"
    ) VALUES (
      NEW."id"
      , NEW."status"
      , NEW."workflow_status"
      , 'status: [' || CAST(OLD."status" AS VARCHAR) || ':' || CAST(NEW."status" AS VARCHAR) ||
        '], workflow_status: [' || CAST(OLD."workflow_status" AS VARCHAR) || ':' || CAST(NEW."workflow_status" AS VARCHAR) || ']'
      , NEW."created_by_id"
      , NEW."created_by"
      , now()
      , OLD."created_by_id"
      , OLD."created_by"
      , now()
      , OLD."version"
    );
  ELSIF (TG_OP = 'INSERT') THEN
    INSERT INTO public.content_log (
      "content_id"
      , "status"
      , "workflow_status"
      , "message"
      , "created_by_id"
      , "created_by"
      , "created_on"
      , "updated_by_id"
      , "updated_by"
      , "updated_on"
      , "version"
    ) VALUES (
      NEW."id"
      , NEW."status"
      , NEW."workflow_status"
      , 'Added'
      , NEW."created_by_id"
      , NEW."created_by"
      , now()
      , NEW."created_by_id"
      , NEW."created_by"
      , now()
      , NEW."version"
    );
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';
CREATE TRIGGER tr_log_content AFTER INSERT OR UPDATE ON public.content FOR EACH ROW EXECUTE PROCEDURE updateContentLog();

CREATE SEQUENCE IF NOT EXISTS public.seq_print_content_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.print_content
(
  "content_id" INT NOT NULL,
  "edition" VARCHAR(100) NOT NULL,
  "section" VARCHAR(100) NOT NULL,
  "story_type" VARCHAR(100) NOT NULL,
  "byline" VARCHAR(500) NOT NULL,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_print_content_version'),
  CONSTRAINT "pk_print_content" PRIMARY KEY ("content_id"),
  CONSTRAINT "fk_content_print_content" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "idx_print_content_edition_section_story_type_page" ON public.print_content ("edition", "section", "story_type");
CREATE INDEX IF NOT EXISTS "idx_print_content_byline" ON public.print_content ("byline");
CREATE TRIGGER tr_audit_print_content BEFORE INSERT OR UPDATE ON public.print_content FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_time_tracking_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_time_tracking_version'),
  CONSTRAINT "pk_time_tracking" PRIMARY KEY ("content_id", "user_id"),
  CONSTRAINT "fk_content_time_tracking" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_user_time_tracking" FOREIGN KEY ("user_id") REFERENCES public.user ("id")
);
CREATE TRIGGER tr_audit_time_tracking BEFORE INSERT OR UPDATE ON public.time_tracking FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_action_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.content_action
(
  "content_id" INT NOT NULL,
  "action_id" INT NOT NULL,
  "value" VARCHAR(250) NOT NULL DEFAULT '',
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_action_version'),
  CONSTRAINT "pk_content_action" PRIMARY KEY ("content_id", "action_id"),
  CONSTRAINT "fk_content_content_action" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_action_content_action" FOREIGN KEY ("action_id") REFERENCES public.action ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_content_action BEFORE INSERT OR UPDATE ON public.content_action FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_tone_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_tone_version'),
  CONSTRAINT "pk_content_tone" PRIMARY KEY ("content_id", "tone_pool_id"),
  CONSTRAINT "fk_content_content_tone" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_tone_pool_content_tone" FOREIGN KEY ("tone_pool_id") REFERENCES public.tone_pool ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_content_tone BEFORE INSERT OR UPDATE ON public.content_tone FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_category_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_category_version'),
  CONSTRAINT "pk_content_category" PRIMARY KEY ("content_id", "category_id"),
  CONSTRAINT "fk_content_content_category" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_category_content_category" FOREIGN KEY ("category_id") REFERENCES public.category ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_content_category BEFORE INSERT OR UPDATE ON public.content_category FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_tag_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_tag_version'),
  CONSTRAINT "pk_content_tag" PRIMARY KEY ("content_id", "tag_id"),
  CONSTRAINT "fk_content_content_tag" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_tag_content_tag" FOREIGN KEY ("tag_id") REFERENCES public.tag ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_content_tag BEFORE INSERT OR UPDATE ON public.content_tag FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_content_link_version AS BIGINT INCREMENT BY 1 START 1;
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
  "version" BIGINT NOT NULL DEFAULT nextval('seq_content_link_version'),
  CONSTRAINT "pk_content_link" PRIMARY KEY ("content_id", "link_id"),
  CONSTRAINT "fk_content_content_link" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE,
  CONSTRAINT "fk_content_content_link_Link" FOREIGN KEY ("link_id") REFERENCES public.content ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_content_link BEFORE INSERT OR UPDATE ON public.content_link FOR EACH ROW EXECUTE PROCEDURE updateAudit();

CREATE SEQUENCE IF NOT EXISTS public.seq_file_reference AS INT INCREMENT BY 1 START 1;
CREATE SEQUENCE IF NOT EXISTS public.seq_file_reference_version AS BIGINT INCREMENT BY 1 START 1;
CREATE TABLE IF NOT EXISTS public.file_reference
(
  "id" INT NOT NULL DEFAULT nextval('seq_file_reference'),
  "content_id" INT NOT NULL,
  "mime_type" VARCHAR(100) NOT NULL,
  "path" VARCHAR(500) NOT NULL,
  "size" INT NOT NULL,
  "running_time" INT NOT NULL DEFAULT 0,
  -- Audit Columns
  "created_by_id" UUID NOT NULL,
  "created_by" VARCHAR(50) NOT NULL,
  "created_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_by_id" UUID NOT NULL,
  "updated_by" VARCHAR(50) NOT NULL,
  "updated_on" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "version" BIGINT NOT NULL DEFAULT nextval('seq_file_reference_version'),
  CONSTRAINT "pk_file_reference" PRIMARY KEY ("id"),
  CONSTRAINT "fk_content_file_reference" FOREIGN KEY ("content_id") REFERENCES public.content ("id") ON DELETE CASCADE
);
CREATE TRIGGER tr_audit_file_reference BEFORE INSERT OR UPDATE ON public.file_reference FOR EACH ROW EXECUTE PROCEDURE updateAudit();

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
DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.content_type (
  "name"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Snippet'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Print'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Radio'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TV'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  'Just In' -- name
  , '' -- value_label
  , 0 -- valueType
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alert' -- name
  , '' -- value_label
  , 0 -- valueType
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Front Page' -- name
  , '' -- value_label
  , 0 -- valueType
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Top Story' -- name
  , '' -- value_label
  , 0 -- valueType
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'On Ticker' -- name
  , '' -- value_label
  , 0 -- valueType
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Non Qualified Subject' -- name
  , '' -- value_label
  , 0 -- valueType
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Commentary' -- name
  , 'Commentary Timeout' -- value_label
  , 2 -- valueType
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

INSERT INTO public.content_type_action (
  "content_type_id"
  , "action_id"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  2
  , 1
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1
  , 2
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1
  , 3
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1
  , 4
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1
  , 5
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1
  , 6
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1
  , 7
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

INSERT INTO public.media_type (
  "name"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Syndication' -- 1
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Television' -- 2
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Talk Radio' -- 3
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'News Radio' -- 4
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CC News' -- 5
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CP News' -- 6
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'XML' -- 7
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Regional' -- 8
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Social Media' -- 9
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'AV Archive' -- 10
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Today''s Edition' -- 11
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Transcript' -- 12
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Scrum' -- 13
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Webcast' -- 14
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'HTML' -- 15
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Image' -- 16
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

INSERT INTO public.data_location (
  "name"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Internet' -- 1
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Streaming' -- 2
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'SFTP' -- 3
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'NAS' -- 4
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Desktop' -- 5
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Special Expire'
  , 150 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Never Expire'
  , 0 -- ttl
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  'Continuos - 60s'
  , 60000 -- delayMS
  , 0 -- repeat
  , 0 -- runOnWeekDays
  , 0 -- runOnMonths
  , 0 -- dayOfMonth
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

INSERT INTO public.data_source (
  "name"
  , "code"
  , "description"
  , "is_enabled"
  , "media_type_id"
  , "data_location_id"
  , "license_id"
  , "topic"
  , "connection"
  , "parent_id"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Daily Hive'
  , 'DAILYHIVE'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"http://dailyhive.com/feed/vancouver" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJCN'
  , 'CJCN'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Georgia Straight'
  , 'GEORGIA STRAIGHT'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"http://www.straight.com/xml/feeds/bcg/news" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burnaby Now'
  , 'BNOW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chilliwack Times'
  , 'CTIMES'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tri-Cities Now'
  , 'TCNOW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Record (New Westminster)'
  , 'NWR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Richmond News'
  , 'RNEWS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castanet'
  , 'CASTANET'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"https://www.castanet.net/rss/topheadlines.xml" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Coast Mountain News'
  , 'CMN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Salmon Arm Lakeshore News'
  , 'SALN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Indo-Canadian Voice'
  , 'ICV'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Macleans'
  , 'MACL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iPolitics'
  , 'IPOLY'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"http://www.ipolitics.ca/custom-feeds/bc-gov-feed.php" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cranbrook Townsman'
  , 'CDT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Delta Optimist'
  , 'DO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Langley Advance Times'
  , 'LA'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nelson Star'
  , 'NS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Shore News'
  , 'NSN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Rossland News'
  , 'RN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey Now-Leader'
  , 'SURN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Royal City Record'
  , 'RCR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Announcement'
  , 'ANNOUNCE'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Media Availability'
  , 'MEDAV'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Scrum'
  , 'SCRUM'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Speech'
  , 'SPEECH'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cloverdale Reporter'
  , 'CRR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Province'
  , 'PROVINCE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Creston Valley Advance'
  , 'CVA'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Hook'
  , 'HOOK'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHKG'
  , 'CHKG'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV Online'
  , 'CTV ONLINE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Centre for Policy Alternatives'
  , 'CCPA'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cowichan Valley Citizen'
  , 'CVC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Haida Gwaii Observer'
  , 'HGO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Delta Reporter'
  , 'NDR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tofino-Ucluelet Westerly News'
  , 'TUWN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BNN'
  , 'BNN'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Trail Daily Times'
  , 'TDT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Comox Valley Echo'
  , 'CVE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Abbotsford Times'
  , 'AT'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Edmonton Journal'
  , 'EJ'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Calgary Herald'
  , 'CH'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Oliver Chronicle'
  , 'APOC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'South Okanagan Times Chronicle'
  , 'TTC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Coast Reporter'
  , 'CORE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Dawson Creek Mirror'
  , 'DCMR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Columbia Valley Pioneer'
  , 'CVP'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chemainus Valley Courier'
  , 'CHVC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Opinion 250'
  , 'O250'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Fernie Free Press'
  , 'TFP'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Keremeos Review'
  , 'KR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFTV'
  , 'CFTV'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB'
  , 'CJVB'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ming Pao News'
  , 'MING PAO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sing Tao Daily'
  , 'SING TAO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver is Awesome'
  , 'VIAWE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KRPI'
  , 'KRPI'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Calgary Herald (Print Edition)'
  , 'CHPE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Business in Vancouver'
  , 'BIV'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"http://biv.com/rss" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHBC'
  , 'CHBC'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFJC'
  , 'CFJC'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNL'
  , 'CHNL'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFNR'
  , 'CFNR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKYE'
  , 'CKYE'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Bowen Island Undercurrent'
  , 'BIU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burns Lake Lakes District News'
  , 'BLLDN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Chilliwack Progress'
  , 'CP'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kelowna Capital News'
  , 'KCN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Monday Magazine'
  , 'MM'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island Gazette'
  , 'NIG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sicamouse Eagle Valley News'
  , 'SEVN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Similkameen Spotlight'
  , 'SIMSP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Prince Rupert)'
  , 'PRDN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV'
  , 'CTV'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castlegar News'
  , 'CN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Hope Standard'
  , 'HS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island MidWeek'
  , 'NIMW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince Rupert Northern View'
  , 'NV'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alberni Valley News'
  , 'AVN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Valley Sentinel'
  , 'VS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Trail-Rossland News'
  , 'TRN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Courier'
  , 'VC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kootenay Western Star'
  , 'KWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Sun'
  , 'SUN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Times Colonist (Victoria)'
  , 'TC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX'
  , 'CFAX'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'AV Archive'
  , 'ARCHIVE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Regional'
  , 'REGIONAL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW'
  , 'CKNW'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHEK'
  , 'CHEK'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHAN'
  , 'CHAN'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC'
  , 'CBC'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CIVT'
  , 'CIVT'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Globe and Mail'
  , 'GLOBE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"sftp://gamdelivery.globeandmail.ca/", "username":"", "password":"" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Today''s Edition'
  , 'TE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CC News'
  , 'CCNEWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Kamloops)'
  , 'KAMLOOPS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG'
  , 'CKPG'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'SHAW'
  , 'SHAW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'National Post'
  , 'POST'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CIVI'
  , 'CIVI'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince George Citizen'
  , 'PGC'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"http://library.pressdisplay.com/test/qa/Services/AdvancedSearchRssHandler.ashx?srchText=%2a&srchnewspaper=7254&extended=false" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Open Cabinet'
  , 'OPENCABINET'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily Courier (Kelowna)'
  , 'KELOWNA'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKWX'
  , 'CKWX'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Press Theatre'
  , 'PRESS THEATRE'
  , ''
  , true -- is_enabled
  , 13 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '100 Mile House Free Press'
  , '100MILE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Abbotsford News'
  , 'ABBNEWS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Agassiz-Harrison Observer'
  , 'AGASSIZ'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Aldergrove Star'
  , 'ALDERSTAR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Arrow Lakes News'
  , 'ARROWLAKE'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ashcroft Cache Creek Journal'
  , 'ASHJOUR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Barriere Star Journal'
  , 'BARRSTARR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Burnaby NewsLeader'
  , 'BNL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Campbell River Mirror'
  , 'CRM'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Clearwater Times'
  , 'CT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Comox Valley Record'
  , 'CCVR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cowichan News Leader Pictorial'
  , 'CNLP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Esquimalt News'
  , 'EN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Fort Saint James Courier'
  , 'FSJC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Golden Star'
  , 'GS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Goldstream News Gazette'
  , 'GG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Houston Today'
  , 'HT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Invermere Valley Echo'
  , 'IVE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kamloops This Week'
  , 'KTW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kitimat Northern Sentinel'
  , 'KS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kootenay News Advertiser'
  , 'KNA'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ladysmith Chronicle'
  , 'LC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lake Cowichan Gazette'
  , 'LCG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lakes District News'
  , 'LDN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Langley Times'
  , 'LT'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Maple Ridge-Pitt Meadows News'
  , 'MRN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Merritt Herald'
  , 'MH'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Mission City Record'
  , 'MCR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nanaimo News Bulletin'
  , 'NNB'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'New Westminster News Leader'
  , 'NWNL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Island Weekender'
  , 'NIW'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'North Shore Outlook'
  , 'NSO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Oak Bay News'
  , 'OBN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Parksville Qualicum Beach News'
  , 'PQN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peace Arch News'
  , 'PAN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peninsula News Review'
  , 'PNR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Penticton Western News'
  , 'PW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Port Hardy North Island Gazette'
  , 'PHNIG'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Prince George Free Press'
  , 'PGFP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Princeton Similkameen Spotlight'
  , 'PSS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Government and Service Employees'' Union'
  , 'BCGEU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'New Westminster Record'
  , 'NWREC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBYK'
  , 'CBYK'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CorpCal'
  , 'CorpCal'
  , ''
  , true -- is_enabled
  , 16 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'iNFOnews'
  , 'INFONEWS'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Link'
  , 'LINK'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Quesnel Cariboo Observer'
  , 'QCO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Revelstoke Review'
  , 'RTR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Richmond Review'
  , 'RR'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Saanich News'
  , 'SN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Salmon Arm Observer'
  , 'SAO'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Smithers Interior News'
  , 'SIN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sooke News Mirror'
  , 'SNM'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'South Delta Leader'
  , 'SDL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Summerland Review'
  , 'SR'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Surrey North Delta Leader'
  , 'SL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Terrace Standard'
  , 'TSTD'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Chilliwack Progress'
  , 'TCP'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tri-City News'
  , 'TCN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vanderhoof Omineca Express'
  , 'VOE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vernon Morning Star'
  , 'VMS'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria News'
  , 'VN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria Weekend'
  , 'VW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'WestEnder'
  , 'WESTENDER'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Williams Lake Tribune'
  , 'WLT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Online'
  , 'CBCO'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"https://www.cbc.ca/cmlink/rss-topstories" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily News (Nanaimo)'
  , 'NANAIMO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lake Country Calendar'
  , 'LCC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Caledonia Courier'
  , 'CC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFR'
  , 'CKFR'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Nelson Daily News'
  , 'NDN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CP News'
  , 'CPNEWS'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"http://www.commandnews.com/fpweb/fp.dll/$bc-rss/htm/rss/x_searchlist.htm/_drawerid/!default_bc-rss/_profileid/rss/_iby/daj/_iby/daj/_svc/cp_pub/_k/XQkKHjnAUpumRfdr" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gulf Islands Driftwood'
  , 'GID'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFIS'
  , 'CFIS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCV'
  , 'CBCV'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBTK'
  , 'CBTK'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBU'
  , 'CBU'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBYG'
  , 'CBYG'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBUT'
  , 'CBUT'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The New York Times'
  , 'NYT'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Daily Telegraph'
  , 'DTL'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Online'
  , 'CKNW ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Montreal Gazette'
  , 'MG'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Toronto Star'
  , 'TS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKWX Online'
  , 'CKWX ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Seattle PI Online'
  , 'SPIO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Georgia Straight Online'
  , 'GSO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Province Online'
  , 'PROVO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Globe and Mail Online'
  , 'GMO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Victoria Buzz'
  , 'VBUZZ'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"http://www.victoriabuzz.com/feed/" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KXLY'
  , 'KXLY'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global News: BC 1'
  , 'BC 1'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Boundary Creek Times'
  , 'BCT'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kimberley Bulletin'
  , 'KDB'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peachland View'
  , 'PV'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Conservative Party'
  , 'BC CONS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'News Kamloops'
  , 'NEWKAM'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Sierra Club of BC'
  , 'SIERRA'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Webcast'
  , 'WEBCAST'
  , ''
  , true -- is_enabled
  , 14 -- media_type_id
  , 2 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Squamish Chief'
  , 'SC'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Sun Online'
  , 'SUN ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'HuffPostBC'
  , 'HUFFPOSTBC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB Online'
  , 'CJVB ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKSP'
  , 'CKSP'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Orca'
  , 'ORCA'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"https://theorca.ca/feed/" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKVU'
  , 'CKVU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBX'
  , 'CBX'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kelowna Westside Weekly'
  , 'KWW'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKSP Sameer Kaushal'
  , 'CKSP SAMEER'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Alaska Highway News'
  , 'AHN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Northerner'
  , 'FSJN'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFU'
  , 'CKFU'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver 24 hrs'
  , '24HRS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'StarMetro'
  , 'STARMETRO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Union of BC Indian Chiefs'
  , 'UBCIC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TVS - Talentvision'
  , 'TVS'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CityCaucus.com'
  , 'CITYC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Hospital Employees'' Union'
  , 'HEU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Health Coalition'
  , 'BCHC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHMB'
  , 'CHMB'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJRJ'
  , 'CJRJ'
  , ''
  , true -- is_enabled
  , 3 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'KVRI'
  , 'KVRI'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'British Columbia Nurses'' Union'
  , 'BCNU'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'PTC'
  , 'PTC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vancouver Island Free Daily'
  , 'VIFD'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The Tyee'
  , 'TYEE'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Canadian Union of Public Employees'
  , 'CUPE BC'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'APTN'
  , 'APTN'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Social Media'
  , 'BCPOLI'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG Online'
  , 'CKPG ONLINE'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNM'
  , 'CHNM'
  , ''
  , true -- is_enabled
  , 2 -- media_type_id
  , 4 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'British Columbia Federation of Labour'
  , 'BC FED'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Grand Forks Gazette'
  , 'GFG'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Teachers'' Federation'
  , 'BCTF'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Public Eye Online'
  , 'PEO'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Local News'
  , 'BCLN'
  , ''
  , true -- is_enabled
  , 7 -- media_type_id
  , 3 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCIndigNews'
  , 'CBCINDIGNEWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCBCNews'
  , 'CBCBCNEWS'
  , ''
  , true -- is_enabled
  , 15 -- media_type_id
  , 5 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{}' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Narwhal'
  , 'NAR'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"https://thenarwhal.ca/feed/rss2" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Infotel'
  , 'INFOTEL'
  , ''
  , true -- is_enabled
  , 1 -- media_type_id
  , 1 -- data_location_id
  , 3 -- license_id
  , 'topic'
  , '{ "url":"https://infotel.ca/govbcrssfeed" }' -- connection
  , NULL -- parent_id
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'editor' -- name
  , 'c374cbb1-7eda-4259-8f74-cd6c2287e32b' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'subscriber' -- name
  , '4818b135-034e-40d8-bd9c-8df2573ce9e0' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Editor' -- name
  , 'd5786a5b-4f71-46f1-9c0c-f6b69ee741b1' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Subscriber' -- name
  , '250eaa7f-67c9-4a60-b453-8ca790d569f5' -- key
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'editor' -- name
  , '1057697d-5cd0-4b00-97f3-df0f13849217' -- key
  , 'editor@local.com' -- email
  , 'Editor' -- displayName
  , true -- emailVerified
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'subscriber' -- name
  , 'e9b0ec48-c4c2-4c73-80a6-5c03da70261d' -- key
  , 'subscriber@local.com' -- email
  , 'Subscriber' -- displayName
  , true -- emailVerified
  , true -- isEnabled
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '2' -- userId
  , '2' -- roleId
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '3' -- userId
  , '3' -- roleId
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '1' -- roleId
  , '2' -- claimId
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '1' -- roleId
  , '3' -- claimId
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '2' -- roleId
  , '2' -- claimId
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  '3' -- roleId
  , '3' -- claimId
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

INSERT INTO public.category (
  "name"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Red tape'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Youth sports'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Schoenborn hearing'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cruise ship season'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TMX purchase'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Overdose crisis'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Single-use fees'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Powell River name change'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ICBC profit'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Forced patient transfers'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Used car tax'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'FOI fees'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC crime'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Farnworth - Lytton recovery'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Highway 99 rockslide'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC housing costs'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gasoline prices'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Inflation concerns'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'UBC Okanagan death'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'COVID-19 restrictions'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Autism funding'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Craig James trial'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Farnworth - sexual assault svcs'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Hospital parking fees'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Khalsa parade cancelled'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Animal feed prices'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'By-election'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'COVID-19 rapid testing'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ukraine invasion'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Russian investments'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

INSERT INTO public.series (
  "name"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES (
  'Jeremy Nuttall'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Bill Tieleman'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Christy Clark'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Editorial'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJCN Vijay Saini'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV Morning Live'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJCN Vasu Kumar'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB News Hotline'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TVS Straits Today'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHKG'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX Noon Show'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'HEU'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX Mornings with Al Ferraby'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BCGEU'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKSP Sameer Kaushal'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Teachers'' Federation'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB Online'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'BC Info Privacy'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Vaughn Palmer'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global BC Noon News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG at 5:00'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHEK News at Five'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cindy E. Harnett'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHBC News at 6:30'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Michael Smyth'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC BC Today'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Early Edition'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'APTN National News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Simi Sara'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Les Leyne'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Weekend Show'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Online'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Mike Smyth'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Newsworld'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Radio West'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC On the Coast'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHEK Political Capital with Rob Shaw'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC On the Island'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFR'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX Ian Jessop'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKWX Online'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC As It Happens'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC The House'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW View From Victoria'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Online'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNL Jeff Andreas'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global BC Morning News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFJC Evening News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC The Current'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV Canada AM'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV News Channel'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFR Phil Johnson'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Keith Baldrey'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHBC News at 5:00'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Daybreak North'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV News Live @ 5'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Daybreak South'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV News Live @ 6'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNL'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Paul Willcocks'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Wendy Stueck'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Stephen Hume'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Press Theatre'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV News at Five'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV News at Six'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC All Points West'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global BC Early News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global BC News Hour'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CityNews1130'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBCV'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBTK'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBU'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBYG'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC News Vancouver'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX Bill Carroll'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX Adam Stirling'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX Ryan Price'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CBC Daybreak Kamloops'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNL Brett Mineer'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNL Paul James'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKYE Harjinder Thind'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Lindsay Kines'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Cassidy Olivier'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Rob Shaw'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Norm Spector'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFAX Evan Solomon'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CUPE BC'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Jas Johal'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFJC Noon News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKPG at Noon'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Global News: BC 1'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHNM Omni Cantonese'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CFTV Fairchild Evening News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CJVB'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CHMB'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Bob Mackin'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKNW Jill Bennett'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'The BC Conservative Party'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CP News'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Dan Burritt'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKFU'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CKYE'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Peter McKnight'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Dirk Meissner'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Don Cayo'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Frances Bula'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gary Mason'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ian Austin'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Gordon McIntyre'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Andrew MacLeod'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Jeff Lee'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tom Fletcher'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ian Mulgrew'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Ian Bailey'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Justine Hunter'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Kim Bolan'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Richard Zussman'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Pete McMartin'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Tracy Sherlock'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Daphne Bramham'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'CTV News at Noon'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'Castanet'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
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
  'EMBC'
  , 'EMBC'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'HLTH'
  , 'HLTH'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'JTST'
  , 'JTST'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ZZA'
  , 'ZZA'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'MCFD'
  , 'MCFD'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'FIN'
  , 'FIN'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ENV'
  , 'ENV'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TNF'
  , 'TNF'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'PSSG'
  , 'PSSG'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'MAZ'
  , 'MAZ'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ADV'
  , 'ADV'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TACZ'
  , 'TACZ'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'MJAG'
  , 'MJAG'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'EDU'
  , 'EDU'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'ZPZ'
  , 'ZPZ'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'EMPR'
  , 'EMPR'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'TRAN'
  , 'TRAN'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'PJJH'
  , 'PJJH'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  'MSD'
  , 'MSD'
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
