CREATE OR REPLACE FUNCTION logContentReference()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT' OR OLD."workflow_status" != NEW."workflow_status") THEN
      INSERT INTO public.content_reference_log (
        "source"
        , "uid"
        , "workflow_status"
        , "message"
        , "created_by_id"
        , "created_by"
        , "created_on"
        , "updated_by_id"
        , "updated_by"
        , "updated_on"
      ) VALUES (
        NEW."source"
        , NEW."uid"
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
CREATE TRIGGER tr_logContentReference AFTER INSERT OR UPDATE ON public.content_reference FOR EACH ROW EXECUTE PROCEDURE logContentReference();
