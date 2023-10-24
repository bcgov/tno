import { IReportForm } from '../interfaces';
import { defaultReportSchedule } from './defaultReportSchedule';

export const defaultReport = (ownerId: number | undefined = 0): IReportForm => {
  var report: IReportForm = {
    id: 0,
    name: '',
    description: '',
    ownerId: ownerId ?? 0,
    templateId: 0,
    isEnabled: true,
    isPublic: false,
    sortOrder: 0,
    hideEmptySections: false,
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
    sections: [],
    subscribers: [],
    instances: [],
    events: [],
  };
  report.events = [
    defaultReportSchedule('Schedule 1', report),
    defaultReportSchedule('Schedule 2', report),
  ];

  return report;
};
