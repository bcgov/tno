DO $$
DECLARE DEFAULT_USER_ID UUID := '00000000-0000-0000-0000-000000000000';
BEGIN

INSERT INTO public.content_type_action (
  "content_type"
  , "action_id"
  , "created_by_id"
  , "created_by"
  , "updated_by_id"
  , "updated_by"
) VALUES

-- *************
-- PrintContent
-- *************
(
  0 -- Snippet
  , 1 -- Alert
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  0 -- Snippet
  , 2 -- Just In
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  0 -- Snippet
  , 3 -- Front Page
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  0 -- Snippet
  , 4 -- Top Story
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  0 -- Snippet
  , 5 -- On Ticker
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  0 -- Snippet
  , 6 -- Non Qualified Subject
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  0 -- Snippet
  , 7 -- Commentary
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
),

-- *************
-- PrintContent
-- *************
(
  1 -- PrintContent
  , 2 -- Just In
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1 -- PrintContent
  , 3 -- Front Page
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1 -- PrintContent
  , 4 -- Top Story
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1 -- PrintContent
  , 5 -- On Ticker
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1 -- PrintContent
  , 6 -- Non Qualified Subject
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
), (
  1 -- PrintContent
  , 7 -- Commentary
  , DEFAULT_USER_ID
  , ''
  , DEFAULT_USER_ID
  , ''
);

END $$;
