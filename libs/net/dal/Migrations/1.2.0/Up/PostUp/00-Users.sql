DO
$BODY$

DECLARE
	tempRow record;
	tempItem json;

BEGIN

	FOR tempRow IN
	    SELECT "id", "preferences", "preferences" -> 'addresses' as "addresses" FROM public."user" WHERE "account_type" = 2
	LOOP
		FOR tempItem IN SELECT * FROM json_array_elements(to_json(tempRow.addresses))
		LOOP

			RAISE NOTICE 'output from space %', tempItem ->> 'userId';

      INSERT INTO public."user_distribution" (
        "user_id"
        , "linked_user_id"
        , "created_by"
        , "updated_by"
      ) VALUES (
        tempRow."id"
        , (tempItem ->> 'userId')::int
        , ''
        , ''
      ) ON CONFLICT DO NOTHING;
		END LOOP;
	END LOOP;

END;

$BODY$ LANGUAGE plpgsql
