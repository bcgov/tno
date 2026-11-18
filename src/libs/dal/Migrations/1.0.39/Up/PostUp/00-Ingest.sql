DO $$

BEGIN

UPDATE public.ingest SET
    configuration = '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
      "path": "processed",
      "papername": "!@PAPER=",
      "headline": "!@HEAD=",
      "summary": "!@ABSTRACT=",
      "story": "!@TEXT=",
      "author": "!@BYLINE=",
      "date": "!@DATE=",
      "lang": "!@LANG=",
      "section": "!@SECTION=",
      "id": "!@IDNUMBER=",
      "tags": "!@LKW=",
      "page": "!@PAGE=",
      "item": "**START-IO-STORY**",
      "dateFmt": "yyyyMMdd",
      "fileFormat": "fms",
      "filePattern": "^(.+)<date>(.+).fms$",
      "dateOffset": 0,
      "sources": "Vancouver Sun=SUN&The Province=PROVINCE&Times Colonist (Victoria)=TC&National Post=POST&Kelowna Daily Courier=KELOWNA&Delta Optimist=DO&North Shore News=NSN&Burnaby Now=BNOW&New Westminster Record=NWR&Richmond News=RNEWS&Alaska Highway News=AHN&Squamish Chief=SC&Merritt Herald=MH&Tri-City News=TCN&Coast Reporter=CORE&Dawson Creek Mirror=DCMR&Kamloops This Week=KTW&Peachland View=PV&Prince George Citizen=PGC&Oliver Chronicle=APOC&Columbia Valley Pioneer=CVP" }'
	WHERE name = 'Meltwater';

END $$;
