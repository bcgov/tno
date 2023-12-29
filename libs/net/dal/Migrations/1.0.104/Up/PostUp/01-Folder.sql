DO $$

BEGIN
    INSERT INTO PUBLIC.FOLDER (
        "owner_id",
        "settings",
        "created_by",
        "updated_by",
        "name",
        "description",
        "is_enabled",
        "sort_order",
        "filter_id"
    ) VALUES (
        1 -- owner_id
        , '{
  "keepAgeLimit": 0
}' -- settings
    , '' -- created_by
    , '' -- updated_by
    , 'Event of the Day' -- name
    , '' -- description
    ,  true -- is_enabled
    ,  0 -- sort_order
    , (SELECT ID FROM PUBLIC."filter" WHERE "name" = 'Event of the Day - Folder Collector' limit 1) -- filter_id
    );
END $$;
