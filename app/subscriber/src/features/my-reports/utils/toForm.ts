import { IReportModel } from 'tno-core';

import { defaultReportSchedule } from '../constants';
import { IReportForm } from '../interfaces';
import { getHideEmpty } from './getHideEmpty';
import { sortContent } from './sortContent';

/**
 * Converts a report model into a form.
 * @param report Latest report information.
 * @param updateSortOrder Whether to update sort order.
 * @returns a new form.
 */
export const toForm = (report: IReportModel, updateSortOrder: boolean = false): IReportForm => {
  return {
    ...report,
    hideEmptySections: getHideEmpty(report.sections),
    sections: report.sections.map((s) => ({
      ...s,
    })),
    events:
      report.events.length === 2
        ? report.events
        : [
            defaultReportSchedule('Schedule 1', report),
            defaultReportSchedule('Schedule 2', report),
          ],
    instances: report.instances.map((i) => ({
      ...i,
      content: sortContent(i.content, updateSortOrder),
    })),
  };
};
