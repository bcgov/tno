DO $$

DECLARE onlineId INT := (SELECT id FROM public.product WHERE name IN ('Online', 'Online Print'));

BEGIN

UPDATE public.ingest SET product_id = onlineId
	WHERE name IN (
        'Victoria Buzz', 'iPolitics', 'CBC Online', 'iNFOnews', 'Castanet', 'Business in Vancouver', 'Daily Hive', 'Narwhal', 'The Tyee', 'The Georgia Straight'
    ) AND product_id != onlineId;

UPDATE public.source SET product_id = onlineId
	WHERE name IN (
        'Victoria Buzz', 'iPolitics', 'CBC Online', 'iNFOnews', 'Castanet', 'Business in Vancouver', 'Daily Hive', 'Narwhal', 'The Tyee', 'The Georgia Straight'
    ) AND (product_id is null OR product_id != onlineId);

END $$;
