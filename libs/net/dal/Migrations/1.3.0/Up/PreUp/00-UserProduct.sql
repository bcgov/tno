DO $$
BEGIN

-- Determine if a user has requested to subscribe to a product.
-- If they have capture it so that we can apply the correct status.

CREATE TEMPORARY TABLE "temp_user_product" (
  "user_id" INT
  , "product_id" INT
  , "status" INT
);

INSERT INTO "temp_user_product" (
  "user_id"
  , "product_id"
  , "status"
)
SELECT
  "user_id"
  , "product_id"
  , CASE
      -- Request subscribe
      WHEN "is_subscribed" = false AND "requested_is_subscribed_status" = true AND "subscription_change_actioned" = false THEN 1
      -- Request cancel
      WHEN "is_subscribed" = true AND "requested_is_subscribed_status" = false AND "subscription_change_actioned" = false THEN 2
      ELSE 0
    END
FROM public."user_product";

END $$;
