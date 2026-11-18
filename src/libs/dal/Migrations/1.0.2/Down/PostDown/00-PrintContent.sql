DO $$
BEGIN

INSERT INTO print_content (
  "content_id"
  , "edition"
  , "section"
  , "byline"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
)
SELECT
  "content_id"
  , "edition"
  , "section"
  , "byline"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
FROM _print_content;

DROP TABLE _print_content;

END $$;
