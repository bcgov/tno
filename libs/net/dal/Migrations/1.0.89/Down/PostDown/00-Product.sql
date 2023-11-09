DO $$
BEGIN

ALTER TABLE public."product" RENAME CONSTRAINT PK_media_type TO PK_product;

END $$;
