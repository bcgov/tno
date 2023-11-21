import { IReportModel } from 'tno-core';

import { defaultReportSchedule } from '../constants';
import { IReportForm } from '../interfaces';
import { getHideEmpty } from './getHideEmpty';
import { sortContent } from './sortContent';

/**
 * TODO: This is an ugly implementation and confusing.
 * Converts a report model into a form.
 * The form is passed in to extract which sections were expanded.
 * @param report Latest report information.
 * @param form The current form information.
 * @param expand Whether to expand sections.
 * @returns a new form.
 */
export const toForm = (
  report: IReportModel,
  form: IReportForm,
  expand: boolean = false,
): IReportForm => {
  return {
    ...report,
    sections: report.sections.map((section, index) => ({
      ...section,
      expand: form.sections.length > index ? form.sections[index].expand : expand,
    })),
    hideEmptySections: getHideEmpty(report.sections),
    events:
      report.events.length === 2
        ? report.events
        : [
            defaultReportSchedule('Schedule 1', report),
            defaultReportSchedule('Schedule 2', report),
          ],
    instances: report.instances.map((i) => ({ ...i, content: sortContent(i.content) })),
  };
};
