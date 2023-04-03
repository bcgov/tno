DO $$
BEGIN

-- When rolling back we lose our page rules.
UPDATE public."topic_score_rule" SET
  "page_min" = NULL
WHERE "page_min" IS NOT NULL;

UPDATE public."topic_score_rule" SET
  "page_max" = NULL
WHERE "page_max" IS NOT NULL;

ALTER TABLE public."topic_score_rule" ALTER COLUMN "page_min" TYPE integer USING ("page_min"::integer);
ALTER TABLE public."topic_score_rule" ALTER COLUMN "page_max" TYPE integer USING ("page_max"::integer);

END $$;
