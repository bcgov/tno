DO $$
BEGIN

UPDATE public."source" SET
  "short_name" = 'Winnipeg'
WHERE "code" = 'APTN';

UPDATE public."source" SET
  "short_name" = 'Toronto'
WHERE "code" = 'BNN';

UPDATE public."source" SET
  "short_name" = 'BiV'
WHERE "code" = 'BIV';

UPDATE public."source" SET
  "short_name" = 'Toronto'
WHERE "code" = 'CBC';

UPDATE public."source" SET
  "short_name" = 'CBC Victoria'
WHERE "code" = 'CBCV';

UPDATE public."source" SET
  "short_name" = 'CBC Kelowna'
WHERE "code" = 'CBTK';

UPDATE public."source" SET
  "short_name" = 'CBC Vancouver'
WHERE "code" = 'CBU';

UPDATE public."source" SET
  "short_name" = 'CBC-TV Vancouver'
WHERE "code" = 'CBUT';

UPDATE public."source" SET
  "short_name" = 'CBC Prince George'
WHERE "code" = 'CBYG';

UPDATE public."source" SET
  "short_name" = 'CBC Kamloops'
WHERE "code" = 'CBYK';

UPDATE public."source" SET
  "short_name" = 'Victoria'
WHERE "code" = 'CFAX';

UPDATE public."source" SET
  "short_name" = 'Pulse FM'
WHERE "code" = 'CFIS';

UPDATE public."source" SET
  "short_name" = 'Kamloops'
WHERE "code" = 'CFJC';

UPDATE public."source" SET
  "short_name" = 'Fairchild TV'
WHERE "code" = 'CFTV';

UPDATE public."source" SET
  "short_name" = 'Global BC - Vancouver'
  , "name" = 'Global TV'
WHERE "code" = 'CHAN';

UPDATE public."source" SET
  "short_name" = 'Kelowna'
WHERE "code" = 'CHBC';

UPDATE public."source" SET
  "short_name" = 'Victoria'
WHERE "code" = 'CHEK';

UPDATE public."source" SET
  "short_name" = 'AM1320'
WHERE "code" = 'CHMB';

UPDATE public."source" SET
  "short_name" = 'Kamloops'
WHERE "code" = 'CHNL';

UPDATE public."source" SET
  "short_name" = 'OMNI Vancouver'
WHERE "code" = 'CHNM';

UPDATE public."source" SET
  "short_name" = 'CTV Two Victoria'
WHERE "code" = 'CIVI';

UPDATE public."source" SET
  "short_name" = 'CTV Vancouver'
WHERE "code" = 'CIVT';

UPDATE public."source" SET
  "short_name" = 'Connect FM'
WHERE "code" = 'CJCN';

UPDATE public."source" SET
  "short_name" = 'Spice Radio'
WHERE "code" = 'CJRJ';

UPDATE public."source" SET
  "short_name" = 'Fairchild Radio'
WHERE "code" = 'CJVB';

UPDATE public."source" SET
  "short_name" = 'Kelowna'
WHERE "code" = 'CKFR';

UPDATE public."source" SET
  "short_name" = 'Fort St John'
WHERE "code" = 'CKFU';

UPDATE public."source" SET
  "short_name" = 'Vancouver'
WHERE "code" = 'CKNW';

UPDATE public."source" SET
  "short_name" = 'Prince George'
WHERE "code" = 'CKPG';

UPDATE public."source" SET
  "short_name" = 'Sher E Punjab'
WHERE "code" = 'CKSP';

UPDATE public."source" SET
  "short_name" = 'Vancouver'
WHERE "code" = 'CKWX';

UPDATE public."source" SET
  "short_name" = 'RED FM Surrey'
WHERE "code" = 'CKYE';

UPDATE public."source" SET
  "short_name" = 'Toronto'
WHERE "code" = 'CTV';

UPDATE public."source" SET
  "short_name" = 'Okanagan'
WHERE "code" = 'INFONEWS';

UPDATE public."source" SET
  "short_name" = 'Okanagan'
WHERE "code" = 'INFOTEL';

UPDATE public."source" SET
  "short_name" = 'Ottawa'
WHERE "code" = 'IPOLY';

UPDATE public."source" SET
  "short_name" = 'Western edition'
WHERE "code" = 'MING PAO';

UPDATE public."source" SET
  "short_name" = 'Western edition'
WHERE "code" = 'SING TAO';

UPDATE public."source" SET
  "name" = 'Squamish Chief'
WHERE "code" = 'SC';

END $$;

