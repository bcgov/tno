DO $$
BEGIN

-- We want the 'not applicable' topic to show up first.
INSERT INTO public."topic" (
  "name"
  , "description"
  , "topic_type"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'Not Applicable' -- name
  , 'There is no relevant topic for this story' -- description
  , 0 -- topic_type
  , true -- is_enabled
  , 0 -- sort_order
  , ''
  , ''
);

-- Copy the old categories into the new topics table.
-- Populate the new table with the duplicate values.
INSERT INTO public."topic" (
  "name"
  , "description"
  , "topic_type"
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
  , "category_type"
  , "is_enabled"
  , 1 -- sort_order
  , "created_by"
  , "created_on"
  , "updated_by"
  , "updated_on"
  , "version"
FROM _category;

-- Link all the content to the correct topics.
INSERT INTO public."content_topic" (
  "content_id"
  , "topic_id"
  , "score"
  , "created_by"
  , "updated_by"
) SELECT
  cc."content_id"
  , t."id"
  , cc."score"
  , cc."created_by"
  , cc."updated_by"
FROM _content_category cc
JOIN _category c ON cc."category_id" = c."id"
JOIN public."topic" t ON c."name" = t."name";

DROP TABLE _category;
DROP TABLE _content_category;

END $$;
