CREATE OR REPLACE FUNCTION logContent()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT' OR OLD."status" != NEW."status" OR OLD."workflow_status" != NEW."workflow_status") THEN
      INSERT INTO public.content_log (
        "content_id"
        , "status"
        , "workflow_status"
        , "message"
        , "created_by_id"
        , "created_by"
        , "created_on"
        , "updated_by_id"
        , "updated_by"
        , "updated_on"
      ) VALUES (
        NEW."id"
        , NEW."status"
        , NEW."workflow_status"
        , (SELECT (CASE WHEN (TG_OP = 'UPDATE') THEN 'Updated' ELSE 'Created' END))
        , NEW."created_by_id"
        , NEW."created_by"
        , NEW."updated_on"
        , NEW."updated_by_id"
        , NEW."updated_by"
        , NEW."updated_on"
      );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';

-- Add trigger to table.
CREATE TRIGGER tr_logContent AFTER INSERT OR UPDATE ON public.content FOR EACH ROW EXECUTE PROCEDURE logContent();
