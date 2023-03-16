DO $$
BEGIN

-- Remove foreign key to allow editing the tag primary key.
ALTER TABLE public."content_tag" DROP CONSTRAINT "FK_content_tag_tag_tag_id";

-- Add an identity column that will replace the current primary key.
ALTER TABLE public."tag" ADD COLUMN "_id" INT GENERATED ALWAYS AS IDENTITY;

-- COPY tag Ids with code.
CREATE TEMP TABLE _tags AS
SELECT
  "id"
  , "_id"
  , "code"
FROM public."tag";

-- Firstly, remove PRIMARY KEY attribute of former PRIMARY KEY
ALTER TABLE public."tag" DROP CONSTRAINT "tag_pkey";
ALTER TABLE public."tag" DROP COLUMN "id";
-- Then change column name of  your PRIMARY KEY and PRIMARY KEY candidates properly.
ALTER TABLE public."tag" RENAME COLUMN "_id" TO "id";
-- Lastly set your new PRIMARY KEY
ALTER TABLE public."tag" ADD CONSTRAINT "tag_pkey" PRIMARY KEY ("id");

-- Fix foreign key relationship values.
UPDATE public."content_tag" t1
SET
  "tag_id" = t2."_id"
FROM _tags t2
WHERE t1."tag_id" = t2."id";

-- Re-add foreign key.
ALTER TABLE public."content_tag" ADD CONSTRAINT "FK_content_tag_tag_tag_id" FOREIGN KEY ("tag_id") REFERENCES public."tag" ("id") ON DELETE CASCADE;

DROP TABLE _tags;

END $$;
