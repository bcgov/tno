DO $$
BEGIN

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
  "new_id"
  , "id"
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

ALTER TABLE public.tag ADD PRIMARY KEY ("id");

-- Adjust foreign keys to correct values
UPDATE public.content_tag
SET "tag_id" = _tag.new_id
FROM _tag
WHERE _tag.id = public.content_tag.tag_id::varchar;

ALTER TABLE public.content_tag ADD CONSTRAINT "FK_content_tag_tag_tag_id" FOREIGN KEY ("tag_id") REFERENCES public.tag ("id") ON DELETE CASCADE;

DROP TABLE _tag;

END $$;


