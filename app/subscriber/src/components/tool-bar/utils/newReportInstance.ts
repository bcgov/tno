import {
  IReportInstanceContentModel,
  IReportInstanceModel,
  IReportModel,
  ReportStatusName,
} from 'tno-core';

export const newReportInstance = (
  report: IReportModel,
  content: IReportInstanceContentModel[],
): IReportInstanceModel => {
  return {
    // no instances should exist when this function is called
    id: 1,
    reportId: report.id,
    content: content,
    createdBy: report.createdBy,
    createdOn: report.createdOn,
    status: ReportStatusName.Pending,
    subject: 'new instance generated',
    body: '',
    response: '',
  };
};
