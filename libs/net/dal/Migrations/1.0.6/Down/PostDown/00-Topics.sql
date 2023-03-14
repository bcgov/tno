DO $$
BEGIN

-- Copy the topics into the category table.
INSERT INTO public."category" (
  "name"
  , "description"
  , "category_type"
  , "auto_transcribe"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
  , "version"
) SELECT
  "name"
  , "description"
  , "topic_type"
  , false -- auto_transcribe
  , "is_enabled"
  , 0 -- sort_order
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
  , "version"
FROM _topic
WHERE lower("name") != 'not applicable';

-- Link all the content to the correct categories.
INSERT INTO public."content_category" (
  "content_id"
  , "category_id"
  , "score"
  , "created_by"
  , "updated_by"
) SELECT
  ct."content_id"
  , c."id"
  , ct."score"
  , ct."created_by"
  , ct."updated_by"
FROM _content_topic ct
JOIN _topic t ON ct."topic_id" = t."id"
JOIN public."category" c ON t."name" = c."name";

DROP TABLE _topic;
DROP TABLE _content_topic;

END $$;
