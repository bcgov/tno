DO $$
BEGIN

CREATE TEMP TABLE _print_content (
  "content_id"
  , "edition"
  , "section"
  , "byline"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
) AS
SELECT "id"
  , "edition"
  , "section"
  , "byline"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
FROM public.content
WHERE "edition" <> '';

END $$;
