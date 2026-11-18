DO $$
BEGIN

UPDATE content
SET
  "edition" = _po.edition
  , "section" = _po.section
  , "byline" = _po.byline
FROM _print_content _po WHERE content.id = _po.content_id;

DROP TABLE _print_content;

END $$;
