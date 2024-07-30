import { getReportKind, IReportModel, ReportKindName } from 'tno-core';

import * as styled from './styled';

export interface IReportKindIconProps {
  report?: IReportModel;
  includeDescription?: boolean;
}

export const ReportKindIcon = ({ report, includeDescription }: IReportKindIconProps) => {
  if (!report) return null;

  const instance = report.instances.length ? report.instances[0] : undefined;
  const isActive = !instance?.sentOn;
  const kind = getReportKind(report);

  let reportType:
    | { label: string; className: string; description?: React.ReactNode | string }
    | undefined;
  if (kind === ReportKindName.Auto)
    reportType = {
      label: 'Auto',
      className: 'auto',
      description: (
        <>
          Report draft will compile at the time(s) specified in the schedule.{' '}
          <b>You will need to send it out manually</b>.
        </>
      ),
    };
  else if (kind === ReportKindName.AutoSend)
    reportType = {
      label: 'Auto Send',
      className: 'full-auto',
      description: 'Report will compile and send at the time(s) specified in the schedule.',
    };
  else if (kind === ReportKindName.Manual)
    reportType = {
      label: 'Manual',
      className: 'manual',
      description: 'Report draft must be created manually. Report must also be sent out manually.',
    };

  return (
    <styled.ReportKindIcon className={`report-tag${isActive ? ' active' : ''}`}>
      <div
        className={`report-type ${!!reportType ? reportType.className : ''} ${
          includeDescription ? 'add-margin' : ''
        }`}
      >
        {!!reportType && reportType.label}
      </div>
      <div className="report-description">
        {includeDescription && !!reportType && reportType.description}
      </div>
    </styled.ReportKindIcon>
  );
};
