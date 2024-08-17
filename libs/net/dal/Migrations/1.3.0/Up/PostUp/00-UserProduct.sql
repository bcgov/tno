DO $$

DECLARE product_row RECORD;
BEGIN
-- Go through all products and ensure there are matching links to users who are subscribed.
FOR product_row IN
  SELECT * FROM public."product"
LOOP
  IF (product_row."product_type" = 0) THEN
    -- Reports
    INSERT INTO public."user_product" (
      "user_id"
      , "product_id"
      , "status"
      , "created_by"
      , "updated_by"
    )
    SELECT
      "user_id"
      , product_row."id"
      , 0
      , ''
      , ''
    FROM public."user_report"
    WHERE "report_id" = product_row."target_product_id"
    ON CONFLICT DO NOTHING;
  ELSEIF (product_row."product_type" = 1) THEN
    -- Evening Overview
    INSERT INTO public."user_product" (
      "user_id"
      , "product_id"
      , "status"
      , "created_by"
      , "updated_by"
    )
    SELECT
      "user_id"
      , product_row."id"
      , 0
      , ''
      , ''
    FROM public."user_av_overview"
    WHERE "av_overview_template_type" = product_row."target_product_id"
    ON CONFLICT DO NOTHING;
  ELSEIF (product_row."product_type" = 2) THEN
    -- Notifications
    INSERT INTO public."user_product" (
      "user_id"
      , "product_id"
      , "status"
      , "created_by"
      , "updated_by"
    )
    SELECT
      "user_id"
      , product_row."id"
      , 0
      , ''
      , ''
    FROM public."user_notification"
    WHERE "notification_id" = product_row."target_product_id"
    ON CONFLICT DO NOTHING;
  END IF;
END LOOP;

-- Update user product status based on old implementation.
UPDATE public."user_product"
SET "status" = temp."status"
FROM (
  SELECT *
  FROM "temp_user_product"
) AS temp
WHERE user_product."user_id" = temp."user_id"
  AND user_product."product_id" = temp."product_id";

-- Remove temp table
DROP TABLE "temp_user_product";

END $$;
