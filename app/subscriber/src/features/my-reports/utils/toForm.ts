import { IReportModel } from 'tno-core';

import { defaultReportSchedule } from '../constants';
import { IReportForm } from '../interfaces';
import { getHideEmpty } from './getHideEmpty';
import { sanitizeReport } from './sanitizeReport';
import { sortContent } from './sortContent';

/**
 * Converts a report model into a form.
 * @param report Latest report information.
 * @param updateSortOrder Whether to update sort order.
 * @returns a new form.
 */
export const toForm = (report: IReportModel, updateSortOrder: boolean = false): IReportForm => {
  const form: IReportForm = {
    ...report,
    hideEmptySections: getHideEmpty(report.sections),
    sections: report.sections.map((s) => ({
      ...s,
      settings: {
        ...s.settings,
        removeDuplicateTitles3Days: !!s.settings.removeDuplicateTitles3Days,
      },
    })),
    settings: {
      ...report.settings,
      content: {
        ...report.settings.content,
        removeDuplicateTitles3Days: !!report.settings.content?.removeDuplicateTitles3Days,
      },
    },
    events:
      report.events.length === 2
        ? report.events
        : [
            defaultReportSchedule('Schedule 1', report),
            defaultReportSchedule('Schedule 2', report),
          ],
    instances: report.instances.map((i) => ({
      ...i,
      content: [...i.content],
    })),
  };

  const sanitized = sanitizeReport(form);

  return {
    ...sanitized,
    instances: sanitized.instances.map((instance) => ({
      ...instance,
      content: sortContent(instance.content, updateSortOrder),
    })),
  };
};
