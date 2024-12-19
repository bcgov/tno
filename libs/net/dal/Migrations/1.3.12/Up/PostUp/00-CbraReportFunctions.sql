DO $$
BEGIN

-- CBRA report view --
CREATE OR REPLACE VIEW public.vw_cbra_published_contents
 AS
 SELECT DISTINCT c.id,
    c.source,
    c.source_id,
    c.series_id,
    c.media_type_id,
	c.published_on,
	COALESCE(ss.name, '') as series_name
   FROM content c
     LEFT JOIN source s ON c.source_id = s.id
     LEFT JOIN series ss ON c.series_id = ss.id
     JOIN ( SELECT unnest(string_to_array(setting.value::text, ','::text)::integer[]) AS media_type_id
           FROM setting
          WHERE setting.name::text = 'CBRAReportMediaTypeIds'::text) m ON c.media_type_id = m.media_type_id
  WHERE ((ss.id IS NOT NULL AND ss.is_cbra_source = true) OR (ss.id IS NULL AND s.is_cbra_source = true))
  	AND c.status = 2;


-- CBRA report functions --
CREATE OR REPLACE FUNCTION public.fn_cbra_report_total_excerpts(
	f_from_date date,
	f_to_date date)
    RETURNS TABLE(category character varying, totals numeric) 
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE cbra_unqualified integer;
DECLARE cbra_totals integer;
DECLARE cbra_action_id integer;
begin

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_total_excerpts (
		content_id numeric);
	INSERT INTO temp_table_content_total_excerpts
	SELECT DISTINCT c.id
	FROM public.vw_cbra_published_contents c
	where c.published_on >= f_from_date
	and c.published_on  <= f_to_date;

	select CAST(value AS INTEGER) into cbra_action_id
	from setting
	where name = 'CBRAUnqualifiedActionId'
	limit 1;

	select count(*) into cbra_unqualified
	from content_action ca 
	join temp_table_content_total_excerpts c on ca.content_id = c.content_id
	where ca.action_id = cbra_action_id
	and CASE WHEN ca.value = '' THEN false ELSE CAST(ca.value AS BOOLEAN) END = true;

	select count(*) into cbra_totals
	from temp_table_content_total_excerpts;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_total_excerpts (
		category VARCHAR(255),
		totals numeric);
	INSERT into temp_table_total_excerpts
	SELECT 'Total Number of Excerpts',
		(cbra_totals - cbra_unqualified);

	INSERT into temp_table_total_excerpts
	SELECT 'Total Number of Excerpts which do not meet the definition of Qualified Subject Matter',
		cbra_unqualified;

	INSERT into temp_table_total_excerpts
	SELECT 'Total Number of Excerpts over 10 min.',
	(select count(*)
	from temp_table_content_total_excerpts c
		left join file_reference f on f.content_id = c.content_id
	where COALESCE(f.running_time,0)/60000 >= 10);

	INSERT into temp_table_total_excerpts
	SELECT 'Total',
		cbra_totals;

	return query
	select
		tt.category,
		cast(tt.totals as decimal) as totals
	from temp_table_total_excerpts tt;

	DROP TABLE IF EXISTS temp_table_content_total_excerpts;
	DROP TABLE IF EXISTS temp_table_total_excerpts;

end;
$BODY$;


CREATE OR REPLACE FUNCTION public.fn_cbra_report_total_entries(
	f_from_date date,
	f_to_date date)
    RETURNS TABLE(dayofweek text, totalcount numeric, totalcbra numeric) 
    LANGUAGE 'plpgsql'
AS $BODY$
begin

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_total_table_total_entries AS
    select DATE_PART('dow', published_on) as dayofweek,
		count(*) as totalcount
	from content
	where published_on >= f_from_date
	and published_on  <= f_to_date
	group by DATE_PART('dow', published_on);

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_cbra_total_table_total_entries AS
    select DATE_PART('dow', c.published_on) as dayofweek,
		count(c.*) as totalcount
	from public.vw_cbra_published_contents c
	where c.published_on >= f_from_date
	and c.published_on  <= f_to_date
	group by DATE_PART('dow', c.published_on);

	return query
	select
		CASE 
			WHEN tt.dayofweek=0 THEN 'Sunday'
			WHEN tt.dayofweek=1 THEN 'Monday'
			WHEN tt.dayofweek=2 THEN 'Tuesday'
			WHEN tt.dayofweek=3 THEN 'Wednesday'
			WHEN tt.dayofweek=4 THEN 'Thursday'
			WHEN tt.dayofweek=5 THEN 'Friday'
			WHEN tt.dayofweek=6 THEN 'Saturday'
			ELSE 'other'
		END as dayofweek,
		cast(COALESCE(tt.totalcount,0) as decimal) as totalcount,
		cast(COALESCE(c.totalcount, 0) as decimal) as totalcbra
	from temp_total_table_total_entries tt
	left join temp_cbra_total_table_total_entries c on tt.dayofweek = c.dayofweek; 

	DROP TABLE IF EXISTS temp_total_table_total_entries;
	DROP TABLE IF EXISTS temp_cbra_total_table_total_entries;
end;
$BODY$;


CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_broadcaster(
	f_from_date date,
	f_to_date date)
    RETURNS TABLE(sourcetype character varying, totalrunningtime text, percentageoftotalrunningtime numeric) 
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE total_running_time numeric;
begin

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_by_broadcaster (
		content_id numeric,
		source character varying(100));
	INSERT INTO temp_table_content_by_broadcaster
	SELECT DISTINCT c.id, c.source
	FROM public.vw_cbra_published_contents c
	where c.published_on >= f_from_date
	and c.published_on  <= f_to_date;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_by_broadcaster AS
	select 
		c.source,
		sum(COALESCE(f.running_time,0)) as totalrunningtime
	from temp_table_content_by_broadcaster c
		left join file_reference f on f.content_id = c.content_id
	group by c.source;

	select sum(tt.totalrunningtime) into total_running_time
	from temp_table_by_broadcaster tt;

	return query
	select
		tt.source,
TO_CHAR((cast(COALESCE(tt.totalrunningtime,0) as decimal)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') as totalrunningtime,
COALESCE(tt.totalrunningtime,0) / total_running_time as percentageoftotalrunningtime 
	from temp_table_by_broadcaster tt;

	DROP TABLE IF EXISTS temp_table_content_by_broadcaster;
	DROP TABLE IF EXISTS temp_table_by_broadcaster;

end;
$BODY$;


CREATE OR REPLACE FUNCTION public.fn_cbra_report_totals_by_program(
	f_from_date date,
	f_to_date date)
    RETURNS TABLE(mediatype character varying, sourcetype character varying, series character varying, totalcount bigint, totalrunningtime text, percentageoftotalrunningtime numeric) 
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE total_running_time numeric;
begin

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_content_by_program (
		content_id numeric,
		source character varying(100),
		source_id numeric,
		series_id numeric,
		series_name character varying(100),
		media_type_id integer,
		media_type_name character varying(100));
	INSERT INTO temp_table_content_by_program
	SELECT DISTINCT c.id, c.source, c.source_id, c.series_id,
		c.series_name, c.media_type_id, COALESCE(mm.name, '')
	FROM public.vw_cbra_published_contents c
	join media_type mm on mm.id = c.media_type_id
	where c.published_on >= f_from_date
	and c.published_on  <= f_to_date;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_by_program AS
	select c.media_type_id,
		c.source,
		c.source_id,
		c.series_id,
		c.series_name,
		c.media_type_name,
		count(*) as totalcount,
		sum(COALESCE(f.running_time,0)) as totalrunningtime
	from temp_table_content_by_program c
		left join file_reference f on f.content_id = c.content_id
	group by c.media_type_id, c.source, c.source_id, c.series_id, c.series_name,
		c.media_type_name;

	select sum(tt.totalrunningtime) into total_running_time
	from temp_table_by_program tt;

	return query
	select tt.media_type_name as mediatype,
		tt.source,
		tt.series_name,
		tt.totalcount,
TO_CHAR((cast(COALESCE(tt.totalrunningtime,0) as decimal)/1000)*'1 SECOND'::INTERVAL, 'HH24:MI:SS') as totalrunningtime,
COALESCE(tt.totalrunningtime,0) / total_running_time as percentageoftotalrunningtime 
	from temp_table_by_program tt;

	DROP TABLE IF EXISTS temp_table_content_by_program;
	DROP TABLE IF EXISTS temp_table_by_program;

end;
$BODY$;


CREATE OR REPLACE FUNCTION public.fn_cbra_report_staff_summary(
	f_from_date date,
	f_to_date date)
    RETURNS TABLE(staff text, cbra_hours numeric) 
    LANGUAGE 'plpgsql'
AS $BODY$
begin

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_table_staff_summary AS
    select tt.user_id,
		cast(sum(COALESCE(tt.Effort,0)) as decimal) as cbra_hours
	from time_tracking tt
	join public.vw_cbra_published_contents c on tt.content_id = c.id
	where c.published_on >= f_from_date
	and c.published_on  <= f_to_date
	group by tt.user_id;

	CREATE TEMPORARY TABLE IF NOT EXISTS temp_user_table_staff_summary AS
    select distinct id, first_name, last_name
	from public.user;

	return query
	select u.first_name || ' ' || u.last_name, tt.cbra_hours
	from temp_table_staff_summary tt
	join temp_user_table_staff_summary u on tt.user_id = u.id;

	DROP TABLE temp_table_staff_summary;
	DROP TABLE temp_user_table_staff_summary;
end;
$BODY$;

END $$;
