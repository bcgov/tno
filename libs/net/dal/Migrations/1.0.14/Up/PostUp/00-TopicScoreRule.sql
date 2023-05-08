DO $$
BEGIN

DELETE FROM public."topic_score_rule";

INSERT INTO public."topic_score_rule" (
  "source_id"
  , "series_id"
  , "section"
  , "page_min"
  , "page_max"
  , "has_image"
  , "time_min"
  , "time_max"
  , "char_min"
  , "char_max"
  , "score"
  , "sort_order"
  , "created_on"
  , "created_by"
  , "updated_on"
  , "updated_by"
) VALUES (
  88 -- source_id
  , 71 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '18:00:00' -- time_min
  , '18:02:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 200 -- score
  , 0 -- sort_order
  , '2023-04-07T22:50:11.78464Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093456Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  88 -- source_id
  , 71 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '18:00:00' -- time_min
  , '18:10:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 160 -- score
  , 1 -- sort_order
  , '2023-05-01T17:39:16.290662Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093462Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  88 -- source_id
  , 70 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '17:00:00' -- time_min
  , '17:05:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 100 -- score
  , 2 -- sort_order
  , '2023-05-01T17:40:01.189683Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093464Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  88 -- source_id
  , 71 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '18:10:00' -- time_min
  , '18:45:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 80 -- score
  , 3 -- sort_order
  , '2023-05-01T17:46:52.292107Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093466Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  88 -- source_id
  , 70 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '17:00:00' -- time_min
  , '17:10:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 80 -- score
  , 4 -- sort_order
  , '2023-05-01T17:46:14.301705Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093468Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  88 -- source_id
  , 70 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '17:10:00' -- time_min
  , '17:45:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 40 -- score
  , 5 -- sort_order
  , '2023-05-01T17:53:52.415703Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.09347Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  73 -- source_id
  , 61 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '18:00:00' -- time_min
  , '18:05:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 60 -- score
  , 6 -- sort_order
  , '2023-05-01T17:47:41.148702Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093472Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  73 -- source_id
  , 61 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '18:00:00' -- time_min
  , '18:10:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 50 -- score
  , 7 -- sort_order
  , '2023-05-01T17:48:48.662695Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093474Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  73 -- source_id
  , 61 -- series_id
  , null -- section
  , null -- page_min
  , null -- page_max
  , null -- has_image
  , '18:10:00' -- time_min
  , '18:45:00' -- time_max
  , null -- char_min
  , null -- char_max
  , 25 -- score
  , 8 -- sort_order
  , '2023-05-01T18:01:02.946814Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093478Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  91 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 100 -- score
  , 9 -- sort_order
  , '2023-04-06T20:56:49.235759Z' -- created_on
  , 'JERFOSTE' -- created_by
  , '2023-05-03T22:22:17.093481Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  91 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 90 -- score
  , 10 -- sort_order
  , '2023-05-01T17:42:45.436125Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093482Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  91 -- source_id
  , null -- series_id
  , null -- section
  , 'A2' -- page_min
  , 'A2' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 30 -- score
  , 11 -- sort_order
  , '2023-05-03T22:21:28.785754Z' -- created_on
  , 'SRYCKMAN' -- created_by
  , '2023-05-03T22:22:17.093484Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  91 -- source_id
  , null -- series_id
  , null -- section
  , 'A2' -- page_min
  , 'A2' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 20 -- score
  , 12 -- sort_order
  , '2023-05-03T22:21:28.785761Z' -- created_on
  , 'SRYCKMAN' -- created_by
  , '2023-05-03T22:22:17.093486Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  91 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 40 -- score
  , 13 -- sort_order
  , '2023-05-01T17:56:27.977701Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093488Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  91 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 30 -- score
  , 14 -- sort_order
  , '2023-05-01T17:59:59.445739Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.09349Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  91 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A99' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 20 -- score
  , 15 -- sort_order
  , '2023-05-01T18:11:57.033445Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093492Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  248 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 100 -- score
  , 16 -- sort_order
  , '2023-04-07T22:51:36.156156Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093494Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  248 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 90 -- score
  , 17 -- sort_order
  , '2023-05-01T17:43:23.326703Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093496Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  248 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 40 -- score
  , 18 -- sort_order
  , '2023-05-01T17:56:59.098765Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093498Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  248 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 30 -- score
  , 19 -- sort_order
  , '2023-05-01T18:00:24.284256Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.0935Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  248 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A99' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 10 -- score
  , 20 -- sort_order
  , '2023-05-01T18:12:54.984577Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093502Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  248 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A99' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 20 -- score
  , 21 -- sort_order
  , '2023-05-01T18:13:47.365314Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093504Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  249 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 100 -- score
  , 22 -- sort_order
  , '2023-04-07T22:52:15.262857Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093506Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  249 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 90 -- score
  , 23 -- sort_order
  , '2023-05-01T17:50:52.010594Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093508Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  249 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 40 -- score
  , 24 -- sort_order
  , '2023-05-01T17:55:34.956073Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093509Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  249 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 30 -- score
  , 25 -- sort_order
  , '2023-05-01T17:58:35.430064Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093511Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  249 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A50' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 10 -- score
  , 26 -- sort_order
  , '2023-05-01T18:03:31.514083Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093513Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  249 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A99' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 20 -- score
  , 27 -- sort_order
  , '2023-05-01T18:10:46.144361Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093515Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  247 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 100 -- score
  , 28 -- sort_order
  , '2023-04-06T20:57:23.71239Z' -- created_on
  , 'JERFOSTE' -- created_by
  , '2023-05-03T22:22:17.093517Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  247 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 90 -- score
  , 29 -- sort_order
  , '2023-05-01T17:49:51.323609Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093519Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  247 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 40 -- score
  , 30 -- sort_order
  , '2023-05-01T17:55:12.612794Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093521Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  247 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 30 -- score
  , 31 -- sort_order
  , '2023-05-01T17:57:48.046754Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093523Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  247 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A50' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 25 -- score
  , 32 -- sort_order
  , '2023-05-01T18:01:42.062345Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093524Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  247 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A50' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 10 -- score
  , 33 -- sort_order
  , '2023-05-01T18:02:34.394995Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093526Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  246 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 20 -- score
  , 34 -- sort_order
  , '2023-04-17T20:07:16.381464Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093528Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  246 -- source_id
  , null -- series_id
  , null -- section
  , 'A1' -- page_min
  , 'A1' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 15 -- score
  , 35 -- sort_order
  , '2023-05-01T18:15:20.339797Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.09353Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  246 -- source_id
  , null -- series_id
  , null -- section
  , 'A2' -- page_min
  , 'A3' -- page_max
  , true -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 10 -- score
  , 36 -- sort_order
  , '2023-05-01T18:17:53.339795Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093532Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  246 -- source_id
  , null -- series_id
  , null -- section
  , 'A3' -- page_min
  , 'A3' -- page_max
  , null -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 5 -- score
  , 37 -- sort_order
  , '2023-05-01T18:18:26.199066Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093534Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
),
(
  246 -- source_id
  , null -- series_id
  , null -- section
  , 'A4' -- page_min
  , 'A99' -- page_max
  , false -- has_image
  , null -- time_min
  , null -- time_max
  , null -- char_min
  , null -- char_max
  , 5 -- score
  , 38 -- sort_order
  , '2023-05-01T18:19:24.395888Z' -- created_on
  , 'CJHUNTER' -- created_by
  , '2023-05-03T22:22:17.093536Z' -- updated_on
  , 'SRYCKMAN' -- updated_by
);

END $$;


