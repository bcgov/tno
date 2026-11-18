DO $$
BEGIN

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Building code cool room') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Building code cool room' -- name
    , 'Building code cool room' -- description
    , 1 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Fraser - federal rental funds') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Fraser - federal rental funds' -- name
    , 'Fraser - federal rental funds' -- description
    , 1 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='BC Ferries cancellations') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'BC Ferries cancellations' -- name
    , 'BC Ferries cancellations' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Daycare opposition') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Daycare opposition' -- name
    , 'Daycare opposition' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='EV charging rates') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'EV charging rates' -- name
    , 'EV charging rates' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Hwy 4 closure') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Hwy 4 closure' -- name
    , 'Hwy 4 closure' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='July inflation') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'July inflation' -- name
    , 'July inflation' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Agriculture water shortage') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Agriculture water shortage' -- name
    , 'Agriculture water shortage' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='BC Transit dress code') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'BC Transit dress code' -- name
    , 'BC Transit dress code' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Brown lawn consent') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Brown lawn consent' -- name
    , 'Brown lawn consent' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Cost of living') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Cost of living' -- name
    , 'Cost of living' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Drought') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Drought' -- name
    , 'Drought' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Ferry Creek charges dropped') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Ferry Creek charges dropped' -- name
    , 'Ferry Creek charges dropped' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Gas tax burden') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Gas tax burden' -- name
    , 'Gas tax burden' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Housing shortage') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Housing shortage' -- name
    , 'Housing shortage' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Hullo launch') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Hullo launch' -- name
    , 'Hullo launch' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Hydro outages') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Hydro outages' -- name
    , 'Hydro outages' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Inflation impacts') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Inflation impacts' -- name
    , 'Inflation impacts' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Malahat delays') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Malahat delays' -- name
    , 'Malahat delays' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Missing-middle inaction') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Missing-middle inaction' -- name
    , 'Missing-middle inaction' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Overdose crisis') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Overdose crisis' -- name
    , 'Overdose crisis' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Rest area campers') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Rest area campers' -- name
    , 'Rest area campers' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Tay death report') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Tay death report' -- name
    , 'Tay death report' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='VSB land sale') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'VSB land sale' -- name
    , 'VSB land sale' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Watering fines') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Watering fines' -- name
    , 'Watering fines' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

IF NOT EXISTS (SELECT id FROM public."topic" WHERE NAME='Wildlife conflict') THEN
    INSERT INTO public."topic" (
    "name"
    , "description"
    , "topic_type"
    , "is_enabled"
    , "sort_order"
    , "created_by"
    , "updated_by"
    ) VALUES (
    'Wildlife conflict' -- name
    , 'Wildlife conflict' -- description
    , 0 -- topic_type
    , true -- is_enabled
    , 0 -- sort_order
    , ''
    , ''
    );
END IF;

END $$;
