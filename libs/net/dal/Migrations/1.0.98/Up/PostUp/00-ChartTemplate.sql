DO $$
BEGIN

UPDATE public."chart_template" AS a
SET
  "template" = (SELECT
  	REPLACE(REPLACE("template", '"product"', '"mediaType"'), 'c.Product', 'c.MediaType')
  	FROM public."chart_template"
	WHERE "id" = a."id");

END $$;
