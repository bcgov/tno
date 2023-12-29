DO $$
BEGIN

INSERT INTO public."report_template_chart_template" (
	"report_template_id",
  "chart_template_id",
  "created_by",
  "updated_by")
	VALUES (
    (select id from public."report_template" where "name"  = 'Event of the Day' limit 1) -- report_template_id
    ,(select id from public."chart_template" where "name"  = 'Topic Analysis' limit 1) -- chart_template_id
    , '' -- created_by
    , '' -- updated_by
  );

END $$;
