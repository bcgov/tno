-- Reverse the "Morning Process" seed. Deleting the profile cascades its steps, actions, and runs;
-- the filters are then unreferenced and removed. Safe to run when the profile is already absent.
DELETE FROM automation_profile WHERE id = 2;
DELETE FROM filter WHERE id IN (10, 11, 12, 13, 14, 15);
