DO $$
DECLARE targetSourceId INT;
DECLARE targetSeriesId INT;
BEGIN

-- get rid of all existing records
DELETE FROM public."topic_score_rule";

-- get Source ID for "Global TV"
SELECT id FROM public."source" WHERE "source"."name" = 'Global TV' LIMIT 1 INTO targetSourceId;
-- get Series ID for "Global BC Early News"
SELECT id FROM public."series" WHERE "series"."name" = 'Global BC Early News' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '17:00:00', '17:05:00', NULL, NULL, 100, 0, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '17:00:00', '17:10:00', NULL, NULL, 80, 1, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '17:10:00', '17:45:00', NULL, NULL, 40, 2, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
-- get Series ID for "Global BC News Hour"
SELECT id FROM public."series" WHERE "series"."name" = 'Global BC News Hour' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '18:00:00', '18:02:00', NULL, NULL, 200, 3, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '18:00:00', '18:10:00', NULL, NULL, 160, 4, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '18:10:00', '18:45:00', NULL, NULL, 80, 5, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "CTV"
SELECT id FROM public."source" WHERE "source"."name" = 'CTV' LIMIT 1 INTO targetSourceId;
-- get Series ID for "CTV News Live @ 6"
SELECT id FROM public."series" WHERE "series"."name" = 'CTV News Live @ 6' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '18:00:00', '18:05:00', NULL, NULL, 60, 6, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '18:00:00', '18:10:00', NULL, NULL, 50, 7, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, '18:10:00', '18:45:00', NULL, NULL, 25, 8, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "CFTV"
SELECT id FROM public."source" WHERE "source"."name" = 'CFTV' LIMIT 1 INTO targetSourceId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 50, 9, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "CHNM"
SELECT id FROM public."source" WHERE "source"."name" = 'CHNM' LIMIT 1 INTO targetSourceId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 50, 10, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "CTV"
SELECT id FROM public."source" WHERE "source"."name" = 'CKNW' LIMIT 1 INTO targetSourceId;
-- get Series ID for "CKNW Mike Smyth"
SELECT id FROM public."series" WHERE "series"."name" = 'CKNW Mike Smyth' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 75, 11, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
-- get Series ID for "CKNW Simi Sara"
SELECT id FROM public."series" WHERE "series"."name" = 'CKNW Simi Sara' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 65, 12, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
-- get Series ID for "CKNW View From Victoria"
SELECT id FROM public."series" WHERE "series"."name" = 'CKNW View From Victoria' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 100, 13, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "CBC"
SELECT id FROM public."source" WHERE "source"."name" = 'CBC' LIMIT 1 INTO targetSourceId;
-- get Series ID for "Early Edition"
SELECT id FROM public."series" WHERE "series"."name" = 'Early Edition' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 65, 14, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "CKYE"
SELECT id FROM public."source" WHERE "source"."name" = 'CKYE' LIMIT 1 INTO targetSourceId;
-- get Series ID for "CKYE Harjinder Thind"
SELECT id FROM public."series" WHERE "series"."name" = 'CKYE Harjinder Thind' AND "source_id" = targetSourceId LIMIT 1 INTO targetSeriesId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId,   targetSeriesId, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 75, 15, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "Vancouver Sun"
SELECT id FROM public."source" WHERE "source"."name" = 'Vancouver Sun' LIMIT 1 INTO targetSourceId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', true, NULL, NULL, NULL, NULL, 99, 16, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', NULL, NULL, NULL, NULL, NULL, 90, 17, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A3', 'A3', true, NULL, NULL, NULL, NULL, 40, 18, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A3', 'A3', NULL, NULL, NULL, NULL, NULL, 30, 19, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A4', 'A50', true, NULL, NULL, NULL, NULL, 25, 20, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A4', 'A50', NULL, NULL, NULL, NULL, NULL, 10, 21, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "The Province"
SELECT id FROM public."source" WHERE "source"."name" = 'The Province' LIMIT 1 INTO targetSourceId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', true, NULL, NULL, NULL, NULL, 100, 22, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', NULL, NULL, NULL, NULL, NULL, 90, 23, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A3', 'A3', true, NULL, NULL, NULL, NULL, 40, 24, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A3', 'A3', NULL, NULL, NULL, NULL, NULL, 30, 25, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A4', 'A50', NULL, NULL, NULL, NULL, NULL, 10, 26, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A4', 'A99', true, NULL, NULL, NULL, NULL, 20, 27, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "Times Colonist (Victoria)"
SELECT id FROM public."source" WHERE "source"."name" = 'Times Colonist (Victoria)' LIMIT 1 INTO targetSourceId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', true, NULL, NULL, NULL, NULL, 20, 28, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', NULL, NULL, NULL, NULL, NULL, 15, 29, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A2', 'A3', true, NULL, NULL, NULL, NULL, 10, 30, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A3', 'A3', NULL, NULL, NULL, NULL, NULL, 5, 31, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A4', 'A99', false, NULL, NULL, NULL, NULL, 5, 32, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "Globe and Mail"
SELECT id FROM public."source" WHERE "source"."name" = 'Globe and Mail' LIMIT 1 INTO targetSourceId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, 'A1', 'A1', true, NULL, NULL, NULL, NULL, 99, 33, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, 'A1', 'A1', NULL, NULL, NULL, NULL, NULL, 90, 34, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, 'A2', 'A2', true, NULL, NULL, NULL, NULL, 30, 35, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, 'A2', 'A2', NULL, NULL, NULL, NULL, NULL, 20, 36, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, 'A3', 'A3', true, NULL, NULL, NULL, NULL, 40, 37, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, 'A3', 'A3', NULL, NULL, NULL, NULL, NULL, 30, 38, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES ( targetSourceId, NULL, NULL, 'A4', 'A99', true, NULL, NULL, NULL, NULL, 20, 39, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);

-- get Source ID for "National Post"
SELECT id FROM public."source" WHERE "source"."name" = 'National Post' LIMIT 1 INTO targetSourceId;
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', true, NULL, NULL, NULL, NULL, 100, 40, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A1', 'A1', NULL, NULL, NULL, NULL, NULL, 90, 41, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A3', 'A3', true, NULL, NULL, NULL, NULL, 40, 42, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A3', 'A3', NULL, NULL, NULL, NULL, NULL, 30, 43, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A4', 'A99', NULL, NULL, NULL, NULL, NULL, 10, 44, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);
INSERT INTO public.topic_score_rule (source_id, series_id, section, page_min, page_max, has_image, time_min, time_max, char_min, char_max, score, sort_order, created_by, created_on, updated_by, updated_on, version) VALUES (targetSourceId, NULL, NULL, 'A4', 'A99', true, NULL, NULL, NULL, NULL, 20, 45, 'SRYCKMAN', CURRENT_TIMESTAMP, 'admin', CURRENT_TIMESTAMP, 1);


END $$;