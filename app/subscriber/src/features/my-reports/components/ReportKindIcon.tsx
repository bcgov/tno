import { FaUserClock } from 'react-icons/fa';
import { FaClock, FaUser } from 'react-icons/fa6';
import { getReportKind, IReportModel, ReportKindName } from 'tno-core';

import * as styled from './styled';

export interface IReportKindIconProps {
  report?: IReportModel;
}

export const ReportKindIcon = ({ report }: IReportKindIconProps) => {
  if (!report) return null;

  const instance = report.instances.length ? report.instances[0] : undefined;
  const isActive = !instance?.sentOn;
  const kind = getReportKind(report);

  let icon: React.ReactNode;
  if (kind === ReportKindName.Auto) icon = <FaUserClock title="Auto Report" />;
  else if (kind === ReportKindName.AutoSend) icon = <FaClock title="Auto Send Report" />;
  else if (kind === ReportKindName.Manual) icon = <FaUser title="Manual Report" />;
  else icon = <></>;

  return (
    <styled.ReportKindIcon className={`icon${isActive ? ' active' : ''}`}>
      {icon}
    </styled.ReportKindIcon>
  );
};
