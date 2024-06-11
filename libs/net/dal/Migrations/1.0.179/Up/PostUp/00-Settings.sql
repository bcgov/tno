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
)
VALUES (
  'BasicAlertTemplateId'
  , 'Basic Alert Template Id'
  , '1'
  , true
  , 0
  , ''
  , ''
);

END $$;
