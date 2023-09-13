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
    viewOnWebOnly: false,
    subject: {
      text: '',
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
      excludeReports: [],
      highlightKeywords: false,
      showLinkToStory: false,
      clearFolders: false,
      onlyNewContent: false,
    },
    sections: {
      usePageBreaks: false,
    },
  },
  isEnabled: false,
  isPublic: false,
  sortOrder: 0,
  sections: [],
  subscribers: [],
  instances: [],
  schedules: [defaultReportSchedule('Schedule 1'), defaultReportSchedule('Schedule 2')],
};
