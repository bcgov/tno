
-- update existing records
UPDATE public."minister" SET "description" = 'Premier of British Columbia', "sort_order" = 10, "aliases" = 'Eby, D.Eby', "position" = 'Premier of British Columbia', "updated_on" = CURRENT_TIMESTAMP where "name" = 'David Eby';
UPDATE public."minister" SET "description" = 'Agriculture and Food', "sort_order" = 20, "aliases" = 'Alexis, P.Alexis', "position" = 'Agriculture and Food', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Pam Alexis';
UPDATE public."minister" SET "description" = 'Attorney General', "sort_order" = 30, "aliases" = 'Sharma, N.Sharma', "position" = 'Attorney General', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Niki Sharma';
UPDATE public."minister" SET "description" = 'Children and Family Development', "sort_order" = 40, "aliases" = 'M.Dean', "position" = 'Children and Family Development', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Mitzi Dean';
UPDATE public."minister" SET "description" = 'Minister of State for Child Care', "sort_order" = 65, "aliases" = 'Lore, G.Lore', "position" = 'Minister of State for Child Care', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Grace Lore';
UPDATE public."minister" SET "description" = 'Citizens'' Services', "sort_order" = 50, "aliases" = 'Beare, L.Beare', "position" = 'Citizens'' Services', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Lisa Beare';
UPDATE public."minister" SET "description" = 'Education and Child Care', "sort_order" = 60, "aliases" = 'R.Singh', "position" = 'Education and Child Care', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Rachna Singh';
UPDATE public."minister" SET "description" = 'Emergency Managment and Climate Readiness', "sort_order" = 70, "aliases" = 'B.Ma', "position" = 'Emergency Managment and Climate Readiness', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Bowinn Ma';
UPDATE public."minister" SET "description" = 'Energy, Mines and Low Carbon Innovation', "sort_order" = 80, "aliases" = 'J.Osborne, Osborne', "position" = 'Energy, Mines and Low Carbon Innovation', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Josie Osborne';
UPDATE public."minister" SET "description" = 'Finance', "sort_order" = 100, "aliases" = 'K.Conroy', "position" = 'Finance', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Katrine Conroy';
UPDATE public."minister" SET "description" = 'Forests', "sort_order" = 110, "aliases" = 'Ralston, B.Ralston', "position" = 'Forests', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Bruce Ralston';
UPDATE public."minister" SET "description" = 'Health and Minister Responsible for Francophone Affairs', "sort_order" = 120, "aliases" = 'Dix, A.Dix', "position" = 'Health and Minister Responsible for Francophone Affairs', "updated_on"  = CURRENT_TIMESTAMP  where "name" = 'Adrian Dix';

-- fix name typo on original insert
UPDATE public."minister" SET "name" = 'George Heyman' WHERE "name" = 'George Hayman';
UPDATE public."minister" SET "description" = 'Environment and Climate Change Strategy', "sort_order" = 90, aliases = 'Heyman, G.Heyman', "position" = 'Environment and Climate Change Strategy', "updated_on" = CURRENT_TIMESTAMP WHERE "name" = 'George Heyman';

-- insert new records
INSERT INTO public.minister (
    "name"
    , "description"
    , "sort_order"
    , "aliases"
    , "position"
    , "is_enabled"
    , "created_by"
    , "created_on"
    , "updated_by"
    , "updated_on"
) VALUES ( 'Mable Elmore', 'Parliamentary Secretary for Anti-Racism Initiatives', 310, 'M.Elmore', 'Parliamentary Secretary', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Kevin Falcon, BC United Party', 'Opposition Leader', 700, 'K.Falcon', 'Leader, Opposition Party', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Lana Popham', 'Tourism, Arts, Culture and Sport', 220, 'L.Popham', 'Tourism, Arts, Culture and Sport', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Ravi Kahlon', 'Housing and Government House Leader', 130, 'Kahlon, R.Kahlon', 'Housing and Government House Leader', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Murray Rankin', 'Indigenous Relations and Reconciliation', 140, 'Rankin, M. Rankin', 'Indigenous Relations and Reconciliation', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Brenda Bailey', 'Jobs, Economic Development and Innovation', 150, 'Bailey, B.Bailey', 'Jobs, Economic Development and Innovation', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Jagrup Brar', 'Minister of State for Trade', 155, 'Brar, J.Brar', 'Minister of State for Trade', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Harry Bains', 'Labour', 160, 'Bains, H.Bains', 'Labour', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Jennifer Whiteside', 'Mental Health and Addictions', 170, 'Whiteside, J.Whiteside', 'Mental Health and Addictions', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Anne Kang', 'Municipal Affairs', 180, 'Kang, A.Kang', 'Municipal Affairs', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Selina Robinson', 'Post-Secondary Education and Future Skills', 190, 'Robinson, S.Robinson', 'Post-Secondary Education and Future Skills', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Andrew Mercier', 'Minister of State for Workforce Development', 195, 'Mercier, A.Mercier', 'Minister of State for Workforce Development', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Mike Farnworth', 'Public Safety and Solicitor General (ICBC), and Deputy Premier', 200, 'Farnworth, M.Farnworth', 'Public Safety and Solicitor General (ICBC), and Deputy Premier', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Sheila Malcolmson', 'Social Development and Poverty Reduction', 210, 'Malcolmson, S.Malcolmson', 'Social Development and Poverty Reduction', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Rob Fleming', 'Transportation and Infrastructure (BC Transit and Translink)', 230, 'Fleming, R.Fleming', 'Transportation and Infrastructure (BC Transit and Translink)', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Dan Coulter', 'Minister of State for Infrastructure and Transit', 235, 'Coulter, D.Coulter', 'Minister of State for Infrastructure and Transit', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Nathan Cullen', 'Water, Land and Resource Stewardship (Fisheries)', 240, 'Cullen, N.Cullen', 'Water, Land and Resource Stewardship (Fisheries)', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Susie Chant', 'Parliamentary Secretary for Accessibility  ', 300, 'S.Chant', 'Parliamentary Secretary for Accessibility  ', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Bob D''Eith', 'Parliamentary Secretary for Arts and Film', 320, 'B. D''Eith', 'Parliamentary Secretary for Arts and Film', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Jennifer Rice', 'Parliamentary Secretary for Rural Health', 340, 'J.Rice', 'Parliamentary Secretary for Rural Health', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Aman Singh', 'Parliamentary Secretary for Environment', 350, 'A.Singh', 'Parliamentary Secretary for Environment', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Kelly Greene', 'Parliamentary Secretary for Fisheries and Aquaculture', 360, 'K.Greene', 'Parliamentary Secretary for Fisheries and Aquaculture', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Doug Routley', 'Parliamentary Secretary for Forests', 370, 'D.Routley', 'Parliamentary Secretary for Forests', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Kelli Paddon', 'Parliamentary Secretary for Gender Equity', 380, 'K.Paddon', 'Parliamentary Secretary for Gender Equity', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Janet Routledge', 'Parliamentary Secretary for Labour', 390, 'J.Routledge', 'Parliamentary Secretary for Labour', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Harwinder Sandhu', 'Parliamentary Secretary for Seniors'' Services and Long-Term Care', 410, 'H.Sandhu', 'Parliamentary Secretary for Seniors'' Services and Long-Term Care', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Adam Walker', 'Parliamentary Secretary for Sustainable Economy', 420, 'A.Walker', 'Parliamentary Secretary for Sustainable Economy', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Brittny Anderson', 'Parliamentary Secretary for Tourism and Premier''s Special Advisor on Youth', 430, 'B.Anderson', 'Parliamentary Secretary for Tourism and Premier''s Special Advisor on Youth', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Fin Donnelly', 'Parliamentary Secretary for Watershed Restoration', 440, 'F.Donnelly', 'Parliamentary Secretary for Watershed Restoration', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Megan Dykeman', 'Parliamentary Secretary for Community Development and Non-profits', 330, 'M.Dykeman', 'Parliamentary Secretary for Community Development and Non-profits', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
, ( 'Roly Russell', 'Parliamentary Secretary for Rural Development', 400, 'R.Russell', 'Parliamentary Secretary for Rural Development', true, '', CURRENT_TIMESTAMP, '', CURRENT_TIMESTAMP)
 ON CONFLICT DO NOTHING;
