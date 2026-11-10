import React from 'react';
import {
  FaCircleCheck,
  FaCircleExclamation,
  FaCircleStop,
  FaClock,
  FaRegCircle,
} from 'react-icons/fa6';
import { ReportStatusName, Row } from 'tno-core';

export interface IReportStatusIconProps {
  label?: string;
  status?: ReportStatusName;
  onClick?: React.MouseEventHandler<HTMLDivElement>;
}

export const ReportStatusIcon: React.FC<IReportStatusIconProps> = ({ label, status, onClick }) => {
  const getIcon = React.useCallback((status?: ReportStatusName) => {
    switch (status) {
      case ReportStatusName.Submitted:
        return <FaClock />;
      case ReportStatusName.Accepted:
        return <FaCircleCheck className="accepted" />;
      case ReportStatusName.Completed:
        return <FaCircleCheck className="completed" />;
      case ReportStatusName.Cancelled:
        return <FaCircleStop />;
      case ReportStatusName.Failed:
        return <FaCircleExclamation className="error" />;
      case ReportStatusName.Pending:
      case ReportStatusName.Reopen:
      default:
        return <FaRegCircle />;
    }
  }, []);

  return (
    <Row
      gap="0.5rem"
      nowrap
      alignItems="center"
      className="report-status"
      onClick={(e) => onClick?.(e)}
    >
      {label}
      {getIcon(status)}
    </Row>
  );
};
