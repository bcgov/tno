DO $$
BEGIN

INSERT INTO public."setting" (
  "name"
  , "description"
  , "value"
  , "is_enabled"
  , "sort_order"
  , "created_by"
  , "updated_by"
) VALUES (
  'TranscriptApprovedId' -- name
  , 'The notification ID that will be sent out as a confirmation when a subscriber receives an approved transcript.' -- description
  , (SELECT "id"::varchar(255) FROM public."notification" WHERE "name" = 'Transcript Approved')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."notification" WHERE "name" = 'Transcript Approved');

END $$;
