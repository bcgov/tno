DO $$
BEGIN

UPDATE work_order
SET
  "configuration" = "configuration"::jsonb || concat('{ "contentId": ', _wo.content_id, ' }')::jsonb
FROM _work_order _wo WHERE work_order.id = _wo.id;

DROP TABLE _work_order;

END $$;
