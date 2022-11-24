DO $$
BEGIN

INSERT INTO public.content_type_action (
  "content_type"
  , "action_id"
  , "created_by"
  , "updated_by"
) VALUES

-- *************
-- PrintContent
-- *************
(
  0 -- Snippet
  , 1 -- Alert
  , ''
  , ''
), (
  0 -- Snippet
  , 2 -- Just In
  , ''
  , ''
), (
  0 -- Snippet
  , 3 -- Front Page
  , ''
  , ''
), (
  0 -- Snippet
  , 4 -- Top Story
  , ''
  , ''
), (
  0 -- Snippet
  , 5 -- On Ticker
  , ''
  , ''
), (
  0 -- Snippet
  , 6 -- Non Qualified Subject
  , ''
  , ''
), (
  0 -- Snippet
  , 7 -- Commentary
  , ''
  , ''
),

-- *************
-- PrintContent
-- *************
(
  1 -- PrintContent
  , 2 -- Just In
  , ''
  , ''
), (
  1 -- PrintContent
  , 3 -- Front Page
  , ''
  , ''
), (
  1 -- PrintContent
  , 4 -- Top Story
  , ''
  , ''
), (
  1 -- PrintContent
  , 5 -- On Ticker
  , ''
  , ''
), (
  1 -- PrintContent
  , 6 -- Non Qualified Subject
  , ''
  , ''
), (
  1 -- PrintContent
  , 7 -- Commentary
  , ''
  , ''
);

END $$;
