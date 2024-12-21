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
  dayofweek TEXT
  , totalcount NUMERIC
  , totalcbra NUMERIC
)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_total_table_total_entries AS
  SELECT
    DATE_PART('dow', published_on) AS dayofweek
    , COUNT(*) AS totalcount
	FROM content
	WHERE published_on >= f_from_date
	  AND published_on <= f_to_date
	GROUP BY DATE_PART('dow', published_on);

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_cbra_total_table_total_entries AS
  SELECT
    DATE_PART('dow', c.published_on) AS dayofweek
    , COUNT(c.*) AS totalcount
  FROM public.vw_cbra_published_contents c
  WHERE c.published_on >= f_from_date
    AND c.published_on  <= f_to_date
  GROUP BY DATE_PART('dow', c.published_on);

	RETURN query
	SELECT
		CASE
			WHEN tt.dayofweek=0 THEN 'Sunday'
			WHEN tt.dayofweek=1 THEN 'Monday'
			WHEN tt.dayofweek=2 THEN 'Tuesday'
			WHEN tt.dayofweek=3 THEN 'Wednesday'
			WHEN tt.dayofweek=4 THEN 'Thursday'
			WHEN tt.dayofweek=5 THEN 'Friday'
			WHEN tt.dayofweek=6 THEN 'Saturday'
			ELSE 'other'
		END AS dayofweek
    ,	CAST(COALESCE(tt.totalcount,0) AS DECIMAL) AS totalcount
    ,	CAST(COALESCE(c.totalcount, 0) AS DECIMAL) AS totalcbra
	FROM temp_total_table_total_entries tt
	LEFT JOIN temp_cbra_total_table_total_entries c ON tt.dayofweek = c.dayofweek;

	DROP TABLE IF EXISTS temp_total_table_total_entries;
	DROP TABLE IF EXISTS temp_cbra_total_table_total_entries;
END;
$BODY$;

-- Function: fn_cbra_report_totals_by_broadcaster
CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_broadcaster (
	f_from_date DATE,
	f_to_date DATE
)
RETURNS TABLE(sourcetype CHARACTER varying, totalrunningtime TEXT, percentageoftotalrunningtime NUMERIC)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE total_running_time NUMERIC;
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_by_broadcaster (
		content_id NUMERIC,
		source CHARACTER VARYING(100)
  );
	INSERT INTO temp_table_content_by_broadcaster
	SELECT DISTINCT c.id, c.source
	FROM public.vw_cbra_published_contents c
	WHERE c.published_on >= f_from_date
	  AND c.published_on  <= f_to_date;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_by_broadcaster AS
	SELECT
		c.source,
		SUM(COALESCE(f.running_time,0)) AS totalrunningtime
	FROM temp_table_content_by_broadcaster c
		LEFT JOIN file_reference f ON f.content_id = c.content_id
	GROUP BY c.source;

	SELECT SUM(tt.totalrunningtime) INTO total_running_time
	FROM temp_table_by_broadcaster tt;

	RETURN query
	SELECT
		tt.source,
    TO_CHAR((CAST(COALESCE(tt.totalrunningtime,0) AS DECIMAL)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') AS totalrunningtime,
    COALESCE(tt.totalrunningtime,0) / total_running_time AS percentageoftotalrunningtime
	FROM temp_table_by_broadcaster tt;

	DROP TABLE IF EXISTS temp_table_content_by_broadcaster;
	DROP TABLE IF EXISTS temp_table_by_broadcaster;

END;
$BODY$;

-- Function: fn_cbra_report_totals_by_program
CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_program (
	f_from_date DATE,
	f_to_date DATE
)
RETURNS TABLE (
  mediatype CHARACTER varying
  , sourcetype CHARACTER varying
  , series CHARACTER varying
  , totalcount BIGINT
  , totalrunningtime TEXT
  , percentageoftotalrunningtime NUMERIC
)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE total_running_time NUMERIC;
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_by_program (
		content_id NUMERIC,
		source CHARACTER VARYING(100),
		source_id NUMERIC,
		series_id NUMERIC,
		series_name CHARACTER VARYING(100),
		media_type_id INTEGER,
		media_type_name CHARACTER VARYING(100)
  );
	INSERT INTO temp_table_content_by_program
	SELECT DISTINCT c.id, c.source, c.source_id, c.series_id,
		c.series_name, c.media_type_id, COALESCE(mm.name, '')
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
		count(*) AS totalcount,
		sum(COALESCE(f.running_time,0)) AS totalrunningtime
	FROM temp_table_content_by_program c
		LEFT JOIN file_reference f ON f.content_id = c.content_id
	GROUP BY c.media_type_id, c.source, c.source_id, c.series_id, c.series_name, c.media_type_name;

	SELECT SUM(tt.totalrunningtime) INTO total_running_time
	FROM temp_table_by_program tt;

	RETURN query
	SELECT tt.media_type_name AS mediatype,
		tt.source,
		tt.series_name,
		tt.totalcount,
    TO_CHAR((cast(COALESCE(tt.totalrunningtime,0) AS DECIMAL)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') AS totalrunningtime,
    COALESCE(tt.totalrunningtime,0) / total_running_time AS percentageoftotalrunningtime
	FROM temp_table_by_program tt;

	DROP TABLE IF EXISTS temp_table_content_by_program;
	DROP TABLE IF EXISTS temp_table_by_program;

END;
$BODY$;

END $$;
