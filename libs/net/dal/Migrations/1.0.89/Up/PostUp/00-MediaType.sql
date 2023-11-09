DO $$
BEGIN

ALTER TABLE public."media_type" RENAME CONSTRAINT PK_product TO PK_media_type;

END $$;
