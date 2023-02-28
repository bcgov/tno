DO $$
BEGIN

-- Clone the table
CREATE TEMP TABLE _tag AS TABLE public.tag;

-- Create a sequence to populate the primary key.
ALTER TABLE _tag
ADD "new_id" integer;

CREATE SEQUENCE seq START 1;
UPDATE _tag
SET new_id = nextval('seq');

-- Remove foreign key relationship so that we can edit the values.
ALTER TABLE public.content_tag DROP CONSTRAINT "FK_content_tag_tag_tag_id";

-- Drop and recreate table so that we don't have any issues with indexes or primary keys.
DROP TABLE public.tag;
CREATE TABLE "public"."tag" (
    "id" varchar(6) NOT NULL,
    "created_by" varchar(250) NOT NULL,
    "created_on" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" varchar(250) NOT NULL,
    "updated_on" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "version" int8 NOT NULL DEFAULT 0,
    "name" varchar(100) NOT NULL,
    "description" varchar(2000) NOT NULL DEFAULT ''::character varying,
    "is_enabled" bool NOT NULL,
    "sort_order" int4 NOT NULL DEFAULT 0
);

ALTER TABLE public.tag ALTER COLUMN "id" TYPE integer USING ("id"::integer);
ALTER TABLE public.content_tag ALTER COLUMN "tag_id" TYPE integer USING ("tag_id"::integer);

DROP SEQUENCE seq;

END $$;
