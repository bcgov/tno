DO $$

DECLARE wireId INT := (SELECT id FROM public.product WHERE name = 'CP Wire'); -- product_id

BEGIN

UPDATE public.ingest SET product_id = wireId 
	WHERE product_id IN (
		SELECT id FROM public.product
		WHERE name IN ('Live stream', 'BiV', 'Castanet', 'Daily Hive', 'Ha-Shilth-Sa', 'iPolitics', 'Georgia Straight', 'iNFOnews', 'Advertising Campaign', 'Narwhal', 'The Georgia Straight'))
		AND product_id != wireId;

UPDATE public.source SET product_id = null 
	WHERE name IN ('Daily Hive', 'The Georgia Straight', 'Castanet', 'iPolitics', 'Business in Vancouver', 'iNFOnews', 'Narwhal', 'Ha-Shilth-Sa')
		AND product_id is not null;

DELETE FROM public.product
	WHERE name IN ('Live stream', 'BiV', 'Castanet', 'Daily Hive', 'Ha-Shilth-Sa', 'iPolitics', 'Georgia Straight', 'iNFOnews', 'Advertising Campaign', 'Narwhal', 'The Georgia Straight');	

END $$;
