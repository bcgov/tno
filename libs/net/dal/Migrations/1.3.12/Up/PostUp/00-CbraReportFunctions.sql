DO $$
BEGIN

-- CBRA report view --
CREATE OR REPLACE VIEW public.vw_cbra_published_contents AS
SELECT DISTINCT
  c.id
  , c.source
  , c.source_id
  , c.series_id
  , c.media_type_id
  , c.published_on
  ,	COALESCE(ss.name, '') AS series_name
FROM content c
LEFT JOIN source s ON c.source_id = s.id
LEFT JOIN series ss ON c.series_id = ss.id
JOIN (
    SELECT unnest(string_to_array(setting.value::TEXT, ','::TEXT)::INTEGER[]) AS media_type_id
    FROM setting
    WHERE setting.name::text = 'CBRAReportMediaTypeIds'::TEXT
  ) m ON c.media_type_id = m.media_type_id
WHERE ((ss.id IS NOT NULL AND ss.is_cbra_source = true)
  OR (ss.id IS NULL AND s.is_cbra_source = true))
AND c.status = 2;

-- Function: fn_cbra_report_total_excerpts
CREATE OR REPLACE FUNCTION public.fn_cbra_report_total_excerpts (
    f_from_date DATE
    , f_to_date DATE
  )
  RETURNS TABLE(category CHARACTER varying, totals numeric)
  LANGUAGE 'plpgsql'
AS $BODY$
DECLARE cbra_unqualified integer;
DECLARE cbra_totals integer;
DECLARE cbra_action_id integer;
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_total_excerpts (
    content_id NUMERIC
  );
	INSERT INTO temp_table_content_total_excerpts
	SELECT DISTINCT c.id
	FROM public.vw_cbra_published_contents c
	WHERE c.published_on >= f_from_date
	  AND c.published_on <= f_to_date;

	SELECT
    CAST("value" AS INTEGER) INTO cbra_action_id
	FROM setting
	WHERE "name" = 'CBRAUnqualifiedActionId'
	LIMIT 1;

	SELECT COUNT(*) INTO cbra_unqualified
	FROM content_action ca
	JOIN temp_table_content_total_excerpts c ON ca.content_id = c.content_id
	WHERE ca.action_id = cbra_action_id
	  AND CASE WHEN ca.value = ''
      THEN false
      ELSE CAST(ca.value AS BOOLEAN)
    END = true;

	SELECT COUNT(*) INTO cbra_totals
	FROM temp_table_content_total_excerpts;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_total_excerpts (
		category VARCHAR(255)
    , totals NUMERIC
  );
	INSERT INTO temp_table_total_excerpts
	SELECT
    'Total Number of Excerpts'
    ,	(cbra_totals - cbra_unqualified);

	INSERT INTO temp_table_total_excerpts
	SELECT
    'Total Number of Excerpts which do not meet the definition of Qualified Subject Matter'
    ,	cbra_unqualified;

	INSERT iNto temp_table_total_excerpts
	SELECT
    'Total Number of Excerpts over 10 min.'
    , (
      SELECT COUNT(*)
	    FROM temp_table_content_total_excerpts c
		  LEFT JOIN file_reference f ON f.content_id = c.content_id
	    WHERE COALESCE(f.running_time,0)/60000 >= 10
    );

	INSERT INTO temp_table_total_excerpts
	SELECT
    'Total'
    ,	cbra_totals;

	RETURN query
	SELECT
		tt.category
    ,	CAST(tt.totals AS DECIMAL) AS totals
	FROM temp_table_total_excerpts tt;

	DROP TABLE IF EXISTS temp_table_content_total_excerpts;
	DROP TABLE IF EXISTS temp_table_total_excerpts;

END;
$BODY$;

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

-- Function: fn_cbra_report_totals_by_broadcaster (UPDATED)
CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_broadcaster (
    f_from_date DATE
    , f_to_date DATE
  )
RETURNS TABLE (
    sourcetype CHARACTER varying
    , totalrunningtime TEXT
    , percentageoftotalrunningtime NUMERIC
  )
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE total_running_time NUMERIC;
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_by_broadcaster (
    content_id numeric
    , source character varying(100)
  );
	INSERT INTO temp_table_content_by_broadcaster
	SELECT DISTINCT c.id, c.source
	FROM public.vw_cbra_published_contents c
	WHERE c.published_on >= f_from_date
	  AND c.published_on <= f_to_date;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_by_broadcaster AS
	SELECT
		c.source
    , SUM(COALESCE(f.running_time,0)) AS totalrunningtime
	FROM temp_table_content_by_broadcaster c
		LEFT JOIN file_reference f ON f.content_id = c.content_id
	GROUP BY c.source;

	SELECT SUM(tt.totalrunningtime) INTO total_running_time
	FROM temp_table_by_broadcaster tt;

	RETURN query
	SELECT
		tt.source
    , TO_CHAR((cast(COALESCE(tt.totalrunningtime,0) AS DECIMAL)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') AS totalrunningtime
    , COALESCE(tt.totalrunningtime,0) / total_running_time AS percentageoftotalrunningtime
	FROM temp_table_by_broadcaster tt;

	DROP TABLE IF EXISTS temp_table_content_by_broadcaster;
	DROP TABLE IF EXISTS temp_table_by_broadcaster;

END;
$BODY$;

-- Function: fn_cbra_report_totals_by_program (UPDATED)
CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_program (
    f_from_date DATE
    , f_to_date DATE
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
      content_id numeric
      , source character varying(100)
      , source_id numeric
      , series_id numeric
      , series_name character varying(100)
      , media_type_id integer
      , media_type_name character varying(100)
    );
	INSERT INTO temp_table_content_by_program
	SELECT DISTINCT
    c.id
    , c.source
    , c.source_id
    , c.series_id
    ,	c.series_name
    , c.media_type_id
    , COALESCE(mm.name, '')
	FROM public.vw_cbra_published_contents c
	JOIN media_type mm ON mm.id = c.media_type_id
	WHERE c.published_on >= f_from_date
	AND c.published_on  <= f_to_date;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_by_program AS
	select
    c.media_type_id
    ,	c.source
    ,	c.source_id
    ,	c.series_id
    ,	c.series_name
    ,	c.media_type_name
    ,	count(*) AS totalcount
    ,	sum(COALESCE(f.running_time,0)) AS totalrunningtime
	FROM temp_table_content_by_program c
		LEFT JOIN file_reference f ON f.content_id = c.content_id
	GROUP BY c.media_type_id, c.source, c.source_id, c.series_id, c.series_name, c.media_type_name;

	SELECT sum(tt.totalrunningtime) INTO total_running_time
	FROM temp_table_by_program tt;

	RETURN query
	SELECT
    tt.media_type_name AS mediatype
    ,	tt.source
    ,	tt.series_name
    ,	tt.totalcount
    , TO_CHAR((CAST(COALESCE(tt.totalrunningtime,0) AS DECIMAL)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') AS totalrunningtime
    , COALESCE(tt.totalrunningtime,0) / total_running_time AS percentageoftotalrunningtime
	FROM temp_table_by_program tt;

	DROP TABLE IF EXISTS temp_table_content_by_program;
	DROP TABLE IF EXISTS temp_table_by_program;

END;
$BODY$;

-- Function: fn_cbra_report_staff_summary
CREATE OR REPLACE FUNCTION public.fn_cbra_report_staff_summary (
    f_from_date DATE
    , f_to_date DATE
  )
RETURNS TABLE(staff TEXT, cbra_hours NUMERIC)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_staff_summary AS
  SELECT
    tt.user_id
    , CAST(SUM(COALESCE(tt.Effort,0)) AS DECIMAL) AS cbra_hours
	FROM time_tracking tt
	JOIN public.vw_cbra_published_contents c ON tt.content_id = c.id
	WHERE c.published_on >= f_from_date
	  AND c.published_on <= f_to_date
	GROUP BY tt.user_id;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_user_table_staff_summary AS
  SELECT DISTINCT
    id
    , first_name
    , last_name
	FROM public.user;

	RETURN query

	SELECT
    u.first_name || ' ' || u.last_name
    , tt.cbra_hours
	FROM temp_table_staff_summary tt
	JOIN temp_user_table_staff_summary u ON tt.user_id = u.id;

	DROP TABLE temp_table_staff_summary;
	DROP TABLE temp_user_table_staff_summary;
END;
$BODY$;

END $$;
