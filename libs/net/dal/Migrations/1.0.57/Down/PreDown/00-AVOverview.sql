DO $$
BEGIN

-- Clear out any data as it would be invalid.
DELETE FROM public."av_overview_section_item";
DELETE FROM public."av_overview_section";
DELETE FROM public."av_overview_instance";
DELETE FROM public."av_overview_template_section_item";
DELETE FROM public."av_overview_template_section";
DELETE FROM public."av_overview_template";

END $$;
