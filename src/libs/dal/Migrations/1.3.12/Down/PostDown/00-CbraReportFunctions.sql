DO $$
BEGIN

DROP FUNCTION IF EXISTS public.fn_cbra_report_total_excerpts(date, date);

DROP FUNCTION IF EXISTS public.fn_cbra_report_total_entries(date, date);

DROP FUNCTION IF EXISTS public.fn_cbra_report_totals_by_broadcaster(date, date);

DROP FUNCTION IF EXISTS public.fn_cbra_report_totals_by_program(date, date);

DROP FUNCTION IF EXISTS public.fn_cbra_report_staff_summary(date, date);

DROP VIEW IF EXISTS public.vw_cbra_published_contents;

END $$;
