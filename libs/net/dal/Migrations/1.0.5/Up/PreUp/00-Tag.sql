DO $$
BEGIN

CREATE TEMP TABLE _tag AS
SELECT "id", "name"
FROM public.tag;

ALTER TABLE _tag
ADD "new_id" integer;

CREATE SEQUENCE seq START 1;
UPDATE _tag
SET new_id = nextval('seq');

-- set id of publig.tag to match id of _tag table
UPDATE public.tag
SET "id" = _tag.new_id
FROM _tag
WHERE _tag.name = public.tag.name;

-- update content_tag table to match new id
UPDATE public.content_tag
SET "tag_id" = _tag.new_id
FROM _tag
WHERE _tag.id = public.content_tag.tag_id;

-- drop foreign key constraint on content_tag table
ALTER TABLE public.content_tag DROP CONSTRAINT "FK_content_tag_tag_tag_id";


ALTER TABLE public.content_tag ALTER COLUMN "tag_id" SET DATA TYPE integer USING "tag_id"::integer;
ALTER TABLE public.tag ALTER COLUMN "id" SET DATA TYPE integer USING "id"::integer;

-- add foreign key constraint on content_tag table
ALTER TABLE public.content_tag ADD CONSTRAINT "FK_content_tag_tag_tag_id" FOREIGN KEY ("tag_id") REFERENCES public.tag ("id") ON DELETE CASCADE;

END $$;