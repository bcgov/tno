DO $$
BEGIN

INSERT INTO public."minister" (
    "name"
    , "description"
    , "created_by"
    , "updated_by"
    , "created_on"
    , "is_enabled"
    , "sort_order"
    , "updated_on"
    , "version"
  ) VALUES (
    'David Eby'
    , 'Premier'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Pam Alexis'
    , 'Minister of Agriculture and Food'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Niki Sharma'
    , 'Attorney General'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Mitzi Dean'
    , 'Minister of Children and Family Development'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Grace Lore'
    , 'Minister of State for Child Care'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Lisa Beare'
    ,'Minister of Citizens Services'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Rachna Singh'
    , 'Minister of Education and Child Care'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Bowinn Ma'
    , 'Minister of Emergency Managment and Climate Readiness'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Josie Osborne'
    , 'Minister of Energy, Mines and Low Carbon Innovation'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'George Heyman'
    , 'Minister of Environment and Climate Change Strategy'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Katrine Conroy'
    , 'Minister of Finance'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Bruce Ralston'
    , 'Minister of Forests'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  ), (
    'Adrian Dix'
    , 'Minister of Health and Minister Responsible for Francophone Affairs'
    , ''
    , ''
    , CURRENT_TIMESTAMP
    , true
    , 0
    , CURRENT_TIMESTAMP
    , 0
  );

END $$;
