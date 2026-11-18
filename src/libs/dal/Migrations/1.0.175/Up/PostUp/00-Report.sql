DO $$
BEGIN

UPDATE public."report" SET
  "name" = 'MMI Morning Media Analysis',
  "settings" = '
  {
  "content": {
    "clearFolders": true,
    "excludeReports": [],
    "onlyNewContent": false,
    "showLinkToStory": false,
    "copyPriorInstance": false,
    "excludeHistorical": false,
    "highlightKeywords": false,
    "clearOnStartNewReport": false,
    "excludeContentInUnsentReport": false
  },
  "subject": {
    "text": "MMI Morning Media Analysis",
    "showTodaysDate": true
  },
  "headline": {
    "showByline": false,
    "showSource": false,
    "showSentiment": false,
    "showShortName": false,
    "showPublishedOn": false
  },
  "sections": {
    "usePageBreaks": false
  }
}
'
WHERE id IN (SELECT value::integer FROM setting WHERE name = 'EventOfTheDayReportId');

END $$;
