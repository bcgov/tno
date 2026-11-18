DO $$
BEGIN

-- Clone the table
CREATE TEMP TABLE _tag AS TABLE public.tag;

-- Alter tables so that we can make changes
ALTER TABLE public.content_tag DROP CONSTRAINT "FK_content_tag_tag_tag_id";
ALTER TABLE public.content_tag ALTER COLUMN "tag_id" SET DATA TYPE varchar USING "tag_id"::varchar;

-- Don't bother trying to change the primary key it causes too many problems
-- Just delete the table because we have a copy.
DROP TABLE public.tag;

-- Create a new table with the correct state.
CREATE TABLE public.tag (
    "id" varchar(6) NOT NULL,
    "created_by" varchar(250) NOT NULL,
    "created_on" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" varchar(250) NOT NULL,
    "updated_on" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "version" int8 NOT NULL DEFAULT 0,
    "name" varchar(100) NOT NULL,
    "description" varchar(2000) NOT NULL DEFAULT ''::character varying,
    "is_enabled" bool NOT NULL,
    "sort_order" int4 NOT NULL DEFAULT 0,
    "code" varchar(15) NOT NULL,
    PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "IX_name12" ON public.tag  ("name");
CREATE UNIQUE INDEX "IX_code1" ON public.tag  ("code");

-- Populate the new table with the duplicate values.
INSERT INTO public.tag (
  "id"
  , "code"
  , "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
  , "version"
) SELECT
  "code"
  , "code"
  , "name"
  , "description"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
  , "version"
FROM _tag;

-- Update the foreign key values.
UPDATE public.content_tag
SET "tag_id" = _tag.code
FROM _tag
WHERE _tag.id::varchar = public.content_tag.tag_id;

ALTER TABLE public.content_tag ADD CONSTRAINT "FK_content_tag_tag_tag_id" FOREIGN KEY ("tag_id") REFERENCES public.tag ("id") ON DELETE CASCADE;

DROP TABLE _tag;

END $$;
