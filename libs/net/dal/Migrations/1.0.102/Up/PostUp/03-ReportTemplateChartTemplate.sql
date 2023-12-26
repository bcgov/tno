DO $$
BEGIN

INSERT INTO public."report_template_chart_template" (
	"report_template_id",
  "chart_template_id",
  "created_by",
  "updated_by")
	VALUES (
    (select id from public."report_template" where "name"  = 'Event of the Day') -- report_template_id
    ,(select id from public."chart_template" where "name"  = 'Count') -- chart_template_id
    , '' -- created_by
    , '' -- updated_by
  );

END $$;
