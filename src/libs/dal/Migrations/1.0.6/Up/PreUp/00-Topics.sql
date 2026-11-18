DO $$
BEGIN

-- Clone the tables to copy data into new tables.
CREATE TEMP TABLE _category AS TABLE public."category";
CREATE TEMP TABLE _content_category AS TABLE public."content_category";

END $$;
