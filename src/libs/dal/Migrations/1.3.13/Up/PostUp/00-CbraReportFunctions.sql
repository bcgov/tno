DO $$
BEGIN

DROP FUNCTION IF EXISTS public.fn_cbra_report_total_entries(date, date);
DROP FUNCTION IF EXISTS public.fn_cbra_report_totals_by_broadcaster(date, date);
DROP FUNCTION IF EXISTS public.fn_cbra_report_totals_by_program(date, date);

-- Function: fn_cbra_report_total_entries
CREATE OR REPLACE FUNCTION public.fn_cbra_report_total_entries (
    f_from_date DATE
    , f_to_date DATE
  )
RETURNS TABLE (
  day_of_week TEXT
  , total_count NUMERIC
  , total_cbra NUMERIC
)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_total_table_total_entries AS
  SELECT
    DATE_PART('dow', published_on) AS day_of_week
    , COUNT(*) AS total_count
	FROM content
	WHERE published_on >= f_from_date
	  AND published_on <= f_to_date
	GROUP BY DATE_PART('dow', published_on);

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_cbra_total_table_total_entries AS
  SELECT
    DATE_PART('dow', c.published_on) AS day_of_week
    , COUNT(c.*) AS total_count
  FROM public.vw_cbra_published_contents c
  WHERE c.published_on >= f_from_date
    AND c.published_on  <= f_to_date
  GROUP BY DATE_PART('dow', c.published_on);

	RETURN query
	SELECT
		CASE
			WHEN tt.day_of_week=0 THEN 'Sunday'
			WHEN tt.day_of_week=1 THEN 'Monday'
			WHEN tt.day_of_week=2 THEN 'Tuesday'
			WHEN tt.day_of_week=3 THEN 'Wednesday'
			WHEN tt.day_of_week=4 THEN 'Thursday'
			WHEN tt.day_of_week=5 THEN 'Friday'
			WHEN tt.day_of_week=6 THEN 'Saturday'
			ELSE 'other'
		END AS day_of_week
    ,	CAST(COALESCE(tt.total_count,0) AS DECIMAL) AS total_count
    ,	CAST(COALESCE(c.total_count, 0) AS DECIMAL) AS total_cbra
	FROM temp_total_table_total_entries tt
	LEFT JOIN temp_cbra_total_table_total_entries c ON tt.day_of_week = c.day_of_week;

	DROP TABLE IF EXISTS temp_total_table_total_entries;
	DROP TABLE IF EXISTS temp_cbra_total_table_total_entries;
END;
$BODY$;

-- Function: fn_cbra_report_totals_by_broadcaster
CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_broadcaster (
	f_from_date DATE,
	f_to_date DATE
)
RETURNS TABLE (
  source_type CHARACTER varying
  , total_running_time TEXT
  , percentage_of_total_running_time NUMERIC
)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE sum_total_running_time NUMERIC;
begin

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_by_broadcaster (
		content_id NUMERIC,
		source CHARACTER varying(100)
  );
	INSERT INTO temp_table_content_by_broadcaster
	SELECT DISTINCT c.id, c.source
	FROM public.vw_cbra_published_contents c
	WHERE c.published_on >= f_from_date
	  AND c.published_on <= f_to_date;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_by_broadcaster AS
	SELECT
		c.source,
		SUM(COALESCE(f.running_time,0)) AS total_running_time
	FROM temp_table_content_by_broadcaster c
		LEFT JOIN file_reference f ON f.content_id = c.content_id
	GROUP BY c.source;

	select sum(COALESCE(tt.total_running_time,0)) INTO sum_total_running_time
	from temp_table_by_broadcaster tt;

	RETURN query
	SELECT
		tt.source
    , TO_CHAR((CAST(COALESCE(tt.total_running_time,0) AS DECIMAL)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') AS total_running_time
    , COALESCE(tt.total_running_time,0) / CASE COALESCE(sum_total_running_time,0)
      WHEN 0 THEN 1
      ELSE COALESCE(sum_total_running_time,0)
      END AS percentage_of_total_running_time
	FROM temp_table_by_broadcaster tt;

	DROP TABLE IF EXISTS temp_table_content_by_broadcaster;
	DROP TABLE IF EXISTS temp_table_by_broadcaster;

END;
$BODY$;

-- Function: fn_cbra_report_totals_by_program
CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_program(
	f_from_date DATE,
	f_to_date DATE)
    RETURNS TABLE(
      media_type CHARACTER varying
      , source_type CHARACTER varying
      , series CHARACTER varying
      , total_count BIGINT
      , total_running_time TEXT
      , percentage_of_total_running_time NUMERIC
    )
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE sum_total_running_time NUMERIC;
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_by_program (
		content_id NUMERIC,
		source CHARACTER VARYING(100),
		source_id NUMERIC,
		series_id NUMERIC,
		series_name CHARACTER VARYING(100),
		media_type_id INTEGER,
		media_type_name CHARACTER VARYING(100));
	INSERT INTO temp_table_content_by_program
	SELECT DISTINCT
    c.id
    , c.source
    , c.source_id
    , c.series_id
    , c.series_name
    , c.media_type_id
    , COALESCE(mm.name, '')
	FROM public.vw_cbra_published_contents c
	JOIN media_type mm ON mm.id = c.media_type_id
	WHERE c.published_on >= f_from_date
	  AND c.published_on <= f_to_date;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_by_program AS
	SELECT
    c.media_type_id,
		c.source,
		c.source_id,
		c.series_id,
		c.series_name,
		c.media_type_name,
		COUNT(*) AS total_count,
		SUM(COALESCE(f.running_time,0)) AS total_running_time
	FROM temp_table_content_by_program c
		LEFT JOIN file_reference f ON f.content_id = c.content_id
	GROUP BY c.media_type_id, c.source, c.source_id, c.series_id, c.series_name, c.media_type_name;

	SELECT SUM(COALESCE(tt.total_running_time,0)) INTO sum_total_running_time
	FROM temp_table_by_program tt;

	RETURN query
	SELECT
    tt.media_type_name AS media_type,
		tt.source,
		tt.series_name,
		tt.total_count,
    TO_CHAR((cast(COALESCE(tt.total_running_time,0) AS DECIMAL)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') AS total_running_time,
    COALESCE(tt.total_running_time,0) / CASE COALESCE(sum_total_running_time,0)
      WHEN 0 THEN 1
      ELSE COALESCE(sum_total_running_time,0)
      END AS percentage_of_total_running_time
	FROM temp_table_by_program tt;

	DROP TABLE IF EXISTS temp_table_content_by_program;
	DROP TABLE IF EXISTS temp_table_by_program;

END;
$BODY$;

END $$;
