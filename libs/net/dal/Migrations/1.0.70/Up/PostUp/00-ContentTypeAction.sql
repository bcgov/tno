DO $$
BEGIN

DELETE FROM public.content_type_action
    WHERE action_id = 4 -- Top Story
        AND content_type = 0 -- AudioVideo
    ;

END $$;
