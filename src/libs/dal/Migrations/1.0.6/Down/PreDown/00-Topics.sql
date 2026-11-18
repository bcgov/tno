DO $$
BEGIN

-- Clone the tables to copy data into new tables.
CREATE TEMP TABLE _topic AS TABLE public."topic";
CREATE TEMP TABLE _content_topic AS TABLE public."content_topic";

END $$;
