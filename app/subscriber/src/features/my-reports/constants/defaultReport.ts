import { IReportForm } from '../interfaces';
import { defaultReportSchedule } from './defaultReportSchedule';

export const defaultReport = (ownerId: number, templateId: number): IReportForm => {
  var report: IReportForm = {
    id: 0,
    name: '',
    description: '',
    ownerId: ownerId,
    templateId,
    isEnabled: true,
    isPublic: false,
    sortOrder: 0,
    hideEmptySections: false,
    settings: {
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
        excludeHistorical: true,
        excludeReports: [],
        highlightKeywords: false,
        showLinkToStory: false,
        clearFolders: false,
        onlyNewContent: false,
        copyPriorInstance: false,
      },
      sections: {
        usePageBreaks: true,
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
