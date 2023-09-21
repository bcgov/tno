import { IReportTemplateModel, ReportTypeName } from 'tno-core';

export const defaultReportTemplate: IReportTemplateModel = {
  id: 0,
  name: '',
  description: '',
  reportType: ReportTypeName.Content,
  subject: '',
  body: '',
  isEnabled: true,
  isPublic: false,
  sortOrder: 0,
  chartTemplates: [],
  settings: {
    viewOnWebOnly: false,
    subject: {
      text: false,
      showTodaysDate: false,
    },
    headline: {
      showSource: false,
      showShortName: false,
      showPublishedOn: false,
      showSentiment: false,
    },
    content: {
      excludeHistorical: false,
      excludeReports: false,
      showLinkToStory: false,
      highlightKeywords: false,
    },
    sections: {
      enable: false,
      usePageBreaks: false,
    },
    enableCharts: false,
  },
};
