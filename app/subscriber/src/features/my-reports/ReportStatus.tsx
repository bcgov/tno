import { ReportStatusName } from 'tno-core';

import * as styled from './styled';
import { getStatus } from './utils';

export interface IReportStatusProps {
  status?: ReportStatusName;
}

export const ReportStatus: React.FC<IReportStatusProps> = ({ status }) => {
  return status ? (
    <styled.ReportStatus className={`report-status${status ? ` ${status}` : ''}`}>
      {status ? getStatus(status) : 'Draft'}
    </styled.ReportStatus>
  ) : null;
};
