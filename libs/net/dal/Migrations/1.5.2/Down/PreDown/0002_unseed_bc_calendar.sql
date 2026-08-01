-- Reverse the "BC Calendar" seed. Deleting the profile cascades its steps, actions, and runs;
-- the filter is then unreferenced and removed. Safe to run when the profile is already absent.
DELETE FROM automation_profile WHERE id = 3;
DELETE FROM filter WHERE id = 19;
