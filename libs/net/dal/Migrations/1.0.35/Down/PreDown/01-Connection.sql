DO $$
BEGIN

DELETE FROM public."connection"
WHERE "id" = (SELECT id FROM public."connection" WHERE "name" = 'Local Volume - Migrated AudioVideo' ORDER BY "id" DESC LIMIT 1);

DELETE FROM public."connection"
WHERE "id" = (SELECT id FROM public."connection" WHERE "name" = 'Local Volume - Migrated Images' ORDER BY "id" DESC LIMIT 1);

DELETE FROM public."connection"
WHERE "id" = (SELECT id FROM public."connection" WHERE "name" = 'Local Volume - Migrated Papers' ORDER BY "id" DESC LIMIT 1);

DELETE FROM public."connection"
WHERE "id" = (SELECT id FROM public."connection" WHERE "name" = 'Local Volume - Migrated Stories' ORDER BY "id" DESC LIMIT 1);

END $$;
