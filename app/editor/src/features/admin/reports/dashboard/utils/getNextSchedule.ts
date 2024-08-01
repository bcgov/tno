import moment from 'moment';
import { IReportModel } from 'tno-core';

import { calcNextScheduleSend } from './calcNextScheduleSend';
import { formatDate } from './formatDate';

export const getNextSchedule = (report: IReportModel) => {
  if (!report.events.length) return undefined;
  if (report.events.every((e) => !e.isEnabled)) return 'Not Scheduled';
  if (report.events.length === 1) {
    const result = calcNextScheduleSend(report, report.events[0]);
    if (result) return result.format('yyyy-MM-DD hh:mm:ssA');
    return undefined;
  }
  const first = calcNextScheduleSend(report, report.events[0]);
  const second = calcNextScheduleSend(report, report.events[1]);
  if (first && second) return moment.min(first, second).format('yyyy-MM-DD hh:mm:ssA');
  if (first) {
    return formatDate(first.format('yyyy-MM-DD hh:mm:ssA'), true);
  }
  if (second) {
    return formatDate(second.format('yyyy-MM-DD hh:mm:ssA'), true);
  }
  return undefined;
};
