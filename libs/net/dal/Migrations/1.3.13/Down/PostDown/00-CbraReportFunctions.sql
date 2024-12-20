DO $$
BEGIN

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

END $$;
