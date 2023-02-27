DO $$
BEGIN

UPDATE public.tag
SET "code" = _tag.id
FROM _tag
WHERE _tag.name = public.tag.name;

ALTER TABLE public.tag
ADD CONSTRAINT constraint_name UNIQUE ("code");

DROP TABLE _tag;
DROP SEQUENCE seq;

END $$;


