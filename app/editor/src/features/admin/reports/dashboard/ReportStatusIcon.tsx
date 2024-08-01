import {
  FaCircleCheck,
  FaCircleExclamation,
  FaCircleStop,
  FaClock,
  FaRegCircle,
} from 'react-icons/fa6';
import { ReportStatusName } from 'tno-core';

export interface IReportStatusIconProps {
  status?: ReportStatusName;
  onClick?: React.MouseEventHandler<SVGElement>;
}

export const ReportStatusIcon: React.FC<IReportStatusIconProps> = ({ status, onClick }) => {
  switch (status) {
    case ReportStatusName.Submitted:
      return <FaClock onClick={onClick} />;
    case ReportStatusName.Accepted:
    case ReportStatusName.Completed:
      return <FaCircleCheck className="success" onClick={onClick} />;
    case ReportStatusName.Cancelled:
      return <FaCircleStop onClick={onClick} />;
    case ReportStatusName.Failed:
      return <FaCircleExclamation className="error" onClick={onClick} />;
    case ReportStatusName.Pending:
    default:
      return <FaRegCircle onClick={onClick} />;
  }
};
