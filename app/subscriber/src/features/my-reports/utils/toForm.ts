import { IReportModel } from 'tno-core';

import { defaultReportSchedule } from '../constants';
import { IReportForm } from '../interfaces';
import { getHideEmpty } from './getHideEmpty';

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
  };
};
