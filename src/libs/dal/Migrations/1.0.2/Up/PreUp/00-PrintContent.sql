DO $$
BEGIN

CREATE TEMP TABLE _print_content AS
SELECT
  "content_id"
  , "edition"
  , "section"
  , "byline"
FROM public.print_content;

END $$;
