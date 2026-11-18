DO $$
BEGIN

-- Clone the tables to copy data into new tables.
CREATE TEMP TABLE _report AS TABLE public."report";

-- Remove all reports so that the table can be updated.
DELETE FROM public."report";

END $$;
