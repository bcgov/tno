DO $$
BEGIN

UPDATE work_order
SET
  "content_id" = _wo."content_id"
FROM _work_order _wo WHERE work_order.id = _wo.id;

DROP TABLE _work_order;

END $$;
