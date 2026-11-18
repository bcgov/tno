DO
$BODY$

DECLARE
	tempRow record;
	tempItem json;
	foundUser boolean;
	emailAddress varchar;
  userId int;

BEGIN

	FOR tempRow IN
    -- Loop through all distribution accounts
    SELECT "id", "preferences", "preferences" -> 'addresses' as "addresses" FROM public."user" WHERE "account_type" = 2
	LOOP
    -- Loop through all distribution email values.
		FOR tempItem IN SELECT * FROM json_array_elements(to_json(tempRow.addresses))
		LOOP
      SELECT (tempItem ->> 'userId') INTO userId;
      SELECT (tempItem ->> 'email') INTO emailAddress;

      -- Check if user exists
      SELECT EXISTS (SELECT * FROM public."user" WHERE "id" = userId) INTO foundUser;
      IF NOT foundUser THEN
        -- Look for a match by email.
        SELECT "id" FROM public."user" WHERE "email" = emailAddress OR "preferred_email" = emailAddress LIMIT 1 INTO userId;
      END IF;

      IF userId IS NOT NULL THEN
		    RAISE NOTICE 'output % % %', tempItem ->> 'userId', userId, foundUser;

        INSERT INTO public."user_distribution" (
          "user_id"
          , "linked_user_id"
          , "created_by"
          , "updated_by"
        ) VALUES (
          tempRow."id"
          , userId
          , ''
          , ''
        ) ON CONFLICT DO NOTHING;
	    END IF;
		END LOOP;
	END LOOP;

END;

$BODY$ LANGUAGE plpgsql
