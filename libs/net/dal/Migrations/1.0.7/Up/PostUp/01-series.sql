DO $$
BEGIN

-- Update series with related source.
UPDATE public."series" s1
SET
  "source_id" = so."id"
FROM public."series" s2
JOIN _series_source ss ON s2."name" = ss."series"
JOIN public."source" so ON ss."source" = so."code"
WHERE s1."id" = s2."id";

DROP TABLE _series_source;

CREATE UNIQUE INDEX "IX_series_name" ON public."series"
("name") WHERE "source_id" IS NULL;

CREATE UNIQUE INDEX "IX_source_id_name" ON public."series"
("source_id", "name") WHERE "source_id" IS NOT NULL;

END $$;
