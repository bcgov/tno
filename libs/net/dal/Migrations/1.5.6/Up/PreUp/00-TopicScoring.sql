-- Scores must be 0 or more (CK_content_topic_score); no valid rule or editor choice is negative.
DO $$
BEGIN

UPDATE public."content_topic"
SET "score" = 0
WHERE "score" < 0;

END $$;
