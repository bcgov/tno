DO $$

BEGIN

UPDATE public.ingest SET
    configuration = '{
      "url":"https://dailyhive.com/feed/vancouver",
      "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true 
      }'
	WHERE name = 'Daily Hive';

END $$;
