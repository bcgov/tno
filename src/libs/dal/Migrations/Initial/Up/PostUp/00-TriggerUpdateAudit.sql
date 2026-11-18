-- Ensure the created audit columns are not changed.
-- Ensure the updated timestamp is updated.
CREATE OR REPLACE FUNCTION updateAudit()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'UPDATE') THEN
        NEW."created_on" = OLD."created_on";
        NEW."created_by" = OLD."created_by";
    ELSIF (TG_OP = 'INSERT') THEN
        NEW."created_on" = now();
    END IF;
    NEW."updated_on" = now();
    RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';
