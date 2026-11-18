DO $$
BEGIN

-- Step 1: Update the boolean fields on the surviving records (oldest ID per duplicate group)
WITH groups AS (
  SELECT
    TRIM(name) AS trimmed_name,
    source_id,
    MIN(id) AS survivor_id,
    BOOL_OR(auto_transcribe) AS new_auto_transcribe,  -- OR: true if any is true
    BOOL_OR(use_in_topics) AS new_use_in_topics,
    BOOL_OR(is_cbra_source) AS new_is_cbra_source,
    BOOL_AND(is_other) AS new_is_other,  -- AND: true only if all are true
    BOOL_OR(is_enabled) AS new_is_enabled  -- OR: true if any is true
  FROM series
  GROUP BY TRIM(name), source_id
  HAVING COUNT(*) > 1
)
UPDATE series s
SET
  auto_transcribe = g.new_auto_transcribe,
  use_in_topics = g.new_use_in_topics,
  is_cbra_source = g.new_is_cbra_source,
  is_other = g.new_is_other,
  is_enabled = g.new_is_enabled
FROM groups g
WHERE s.id = g.survivor_id;

-- Step 2: Create temp table for mappings
CREATE TEMP TABLE mappings (old_id int, new_id int);

INSERT INTO mappings (old_id, new_id)
WITH groups AS (
  SELECT
    TRIM(name) AS trimmed_name,
    source_id,
    MIN(id) AS survivor_id,
    ARRAY_AGG(id ORDER BY id) AS all_ids
  FROM series
  GROUP BY TRIM(name), source_id
  HAVING COUNT(*) > 1
),
all_duplicates AS (
  SELECT
    survivor_id,
    unnest(all_ids) AS id
  FROM groups
)
SELECT
  id,
  survivor_id
FROM all_duplicates
WHERE id != survivor_id;

-- Step 3: Update foreign keys in related tables
UPDATE content c
SET series_id = m.new_id
FROM mappings m
WHERE c.series_id = m.old_id;

UPDATE series_media_type_search_mapping sm
SET series_id = m.new_id
FROM mappings m
WHERE sm.series_id = m.old_id;

UPDATE av_overview_section aos
SET series_id = m.new_id
FROM mappings m
WHERE aos.series_id = m.old_id;

UPDATE av_overview_template_section aots
SET series_id = m.new_id
FROM mappings m
WHERE aots.series_id = m.old_id;

UPDATE topic_score_rule tsr
SET series_id = m.new_id
FROM mappings m
WHERE tsr.series_id = m.old_id;

-- Step 4: Delete the duplicate series records
DELETE FROM series s
USING mappings m
WHERE s.id = m.old_id;

-- Clean up temp table
DROP TABLE mappings;

END $$;
