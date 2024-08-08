import { FaCheckCircle } from 'react-icons/fa';
import { FaBug, FaCirclePause, FaClock, FaRegCircleRight } from 'react-icons/fa6';
import { IWorkOrderModel, Spinner, WorkOrderStatusName } from 'tno-core';

export interface IWorkOrderStatusProps {
  row: IWorkOrderModel;
}

export const WorkOrderStatus: React.FC<IWorkOrderStatusProps> = ({ row }) => {
  if (row.status === WorkOrderStatusName.InProgress)
    return <Spinner size="8px" title="Transcribing" />;
  if (row.status === WorkOrderStatusName.Completed && !row.content?.isApproved)
    return <FaRegCircleRight className="completed" title="Ready to review" />;
  if (row.status === WorkOrderStatusName.Completed && row.content?.isApproved)
    return <FaCheckCircle className="completed" title="Completed" />;
  if (row.status === WorkOrderStatusName.Submitted)
    return <FaClock className="submitted" title="Submitted" />;
  if (row.status === WorkOrderStatusName.Failed) return <FaBug className="failed" title="Failed" />;
  if (row.status === WorkOrderStatusName.Cancelled)
    return <FaCirclePause className="cancelled" title="Cancelled" />;
  return <div>{row.status}</div>;
};
