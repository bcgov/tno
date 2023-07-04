DO $$

BEGIN

UPDATE public.ingest SET
    configuration = '{ "timeZone": "Pacific Standard Time",
      "language": "en-CA",
      "post": true,
      "import": true,
      "path": "processed",
      "papername": "papername",
      "headline": "headline",
      "summary": "summary",
      "story": "story",
      "author": "author",
      "date": "date",
      "id":"id",
      "item": "bcng",
      "page": "page",
      "section": "category",
      "dateFmt": "MM-dd-yyyy",
      "escapeContent": true,
      "addParent": true,
      "filePattern":" ^bcng-<date>-(.+).xml$",
      "dateOffset": -1,
      "sources": "Maple Ridge-Pitt Meadows News=MRN&100 Mile House Free Press=100MILE&Arrow Lakes News=ARROWLAKE&Ashcroft Cache Creek Journal=ASHJOUR&Barriere Star Journal=BARRSTARR&Boundary Creek Times=BCT&Burns Lake Lakes District News=BLLDN&Caledonia Courier=CC&Castlegar News=CN&Clearwater Times=CT&Coast Mountain News=CMN&Cranbrook Townsman=CDT&Creston Valley Advance=CVA&Sicamous Eagle Valley News=SEVN&Fernie Free Press=TFP&Golden Star=GS&Grand Forks Gazette=GFG&Houston Today=HT&Invermere Valley Echo=IVE&Kamloops This Week=KTW&Kelowna Capital News=KCN&Keremeos Review=KR&Kimberley Bulletin=KDB&Kitimat Northern Sentinel=KS&Kootenay News Advertiser=KNA&Lake Country Calendar=LCC&Salmon Arm Lakeshore News=SALN&Merritt Herald=MH&Nelson Star=NS&North Delta Reporter=NDR&Prince Rupert Northern View=NV&Penticton Western News=PW&Prince George Free Press=PGFP&Quesnel Cariboo Observer=QCO&Revelstoke Review=RTR&Rossland News=RN&Salmon Arm Observer=SAO&Similkameen Spotlight=SIMSP&Smithers Interior News=SIN&Summerland Review=SR&Terrace Standard=TSTD&Trail Daily Times=TDT&Comox Valley Echo=CVE&Vanderhoof Omineca Express=VOE&Vernon Morning Star=VMS&Williams Lake Tribune=WLT&Abbotsford News=ABBNEWS&Agassiz-Harrison Observer=AGASSIZ&Aldergrove Star=ALDERSTAR&Bowen Island Undercurrent=BIU&Chilliwack Times=CTIMES&Cloverdale Reporter=CRR&Hope Standard=HS&Langley Times=LT&Langley Advance Times=LA&Mission City Record=MCR&North Shore Outlook=NSO&Peace Arch News=PAN&Richmond Review=RR&Surrey Now-Leader=SURN&Alberni Valley News=AVN&Campbell River Mirror=CRM&Comox Valley Record=CCVR&Cowichan News Leader Pictorial=CNLP&Cowichan Valley Citizen=CVC&Goldstream News Gazette=GG&Gulf Islands Driftwood=GID&Ladysmith Chronicle=LC&Lake Cowichan Gazette=LCG&Monday Magazine=MM&The Daily News (Nanaimo)=NANAIMO&Nanaimo News Bulletin=NNB&North Island Gazette=NIG&Oak Bay News=OBN&Parksville Qualicum Beach News=PQN&Peninsula News Review=PNR&Saanich News=SN&Sooke News Mirror=SNM&Tofino-Ucluelet Westerly News=TUWN&Victoria News=VN&Vancouver Island Free Daily=VIFD&The Free Press=TFP&Chemainus Valley Courier=CHVC&Agassiz Observer=AGASSIZ&Maple Ridge News=MRN&Chilliwack Progress=CP&The Northern View=NV&Haida Gwaii Observer=HGO&South Okanagan Times-Chronicle=TTC" }'
	WHERE name = 'Blacks Newsgroup';

END $$;
