DO $$
BEGIN

-- Update report settings
UPDATE public."report" SET
  "settings" = '{
    "headline": {
      "showSource": false,
      "showCommonCall": false,
      "showPublishedOn": false,
      "showSentiment": false
    },
    "content": {
      "includeStory": true,
      "showImage": true,
      "showThumbs": false,
      "highlightKeywords": false
    },
    "sections": {
      "hideEmpty": true,
      "usePageBreaks": false
    },
    "instance": {
      "excludeHistorical": false,
      "excludeReports": []
    },
    "viewOnWebOnly": false
  }';

END $$;
