DO $$

BEGIN

DELETE FROM public.series
	WHERE name IN (
	'Andrew MacLeod', 'BCGEU', 'BC Info Privacy', 'BC Teachers'' Federation', 'Bill Tieleman',
	'Bob Mackin', 'Cassidy Olivier', 'Castanet', 'CBC', 'CBC Online',
	'CBCV', 'CBTK', 'CBU', 'CBYG', 'CFAX', 'CHKG', 'Christy Clark', 'Cindy E. Harnett', 'CJVB', 'CKFR',
	'CKFU', 'CKNW', 'CP News', 'CP', 'CUPE BC', 'Dan Burritt', 'Daphne Bramham', 'Dirk Meissner', 'Don Cayo', 'Editorial',
	'Frances Bula', 'Gary Mason', 'Gordon McIntyre', 'HEU', 'Ian Austin',
	'Ian Bailey', 'Justine Hunter', 'Kim Bolan', 'Lindsay Kines', 'Michael Smyth',
	'Norm Spector', 'Paul Willcocks', 'Pete McMartin', 'Peter McKnight', 'Stephen Hume',
	'The BC Conservative Party', 'Tom Fletcher', 'Tracy Sherlock', 'Wendy Stueck'
    );

END $$;
