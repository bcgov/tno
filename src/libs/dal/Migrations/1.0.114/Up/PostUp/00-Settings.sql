DO $$
BEGIN

-- get rid of defunct setting
delete from public."setting" 
where "name" = 'AdvancedSearchMediaTypeGroups';

END $$;
