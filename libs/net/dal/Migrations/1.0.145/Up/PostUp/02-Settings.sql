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
  'TranscriptRequestConfirmationId' -- name
  , 'The notification ID that will be sent out as a confirmation when a subscriber requests a transcript.' -- description
  , (SELECT "id"::varchar(255) FROM public."notification" WHERE "name" = 'Transcript Request Confirmation')
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- created_by
  , '' -- updated_by
)
ON CONFLICT("name") DO UPDATE
SET "value" = (SELECT "id"::varchar(255) FROM public."notification" WHERE "name" = 'Transcript Request Confirmation');

END $$;
