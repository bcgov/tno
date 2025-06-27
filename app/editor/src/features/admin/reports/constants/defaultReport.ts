import { IReportModel } from 'tno-core';

import { defaultReportSchedule } from './defaultReportSchedule';
import { defaultReportTemplate } from './defaultReportTemplate';

export const defaultReport: IReportModel = {
  id: 0,
  name: '',
  description: '',
  ownerId: 0,
  templateId: 0,
  template: defaultReportTemplate,
  settings: {
    from: '',
    subject: {
      text: '',
      showTodaysDate: false,
    },
    headline: {
      showByline: false,
      showSource: false,
      showShortName: false,
      showPublishedOn: false,
      showSentiment: false,
    },
    content: {
      excludeHistorical: false,
      excludeReports: [],
      highlightKeywords: false,
      showLinkToStory: false,
      clearFolders: false,
      onlyNewContent: false,
      copyPriorInstance: false,
      clearOnStartNewReport: true,
      excludeContentInUnsentReport: false,
      omitBCUpdates: false,
    },
    sections: {
      usePageBreaks: false,
    },
    doNotSendEmail: false,
  },
  isEnabled: false,
  isPublic: false,
  sortOrder: 0,
  sections: [],
  subscribers: [],
  instances: [],
  events: [defaultReportSchedule('Schedule 1'), defaultReportSchedule('Schedule 2')],
};
