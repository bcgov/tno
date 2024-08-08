import { useTab } from 'components/tab-control';
import { FaPen, FaStop } from 'react-icons/fa6';
import { IWorkOrderModel, Row, Show, WorkOrderStatusName } from 'tno-core';

export interface IWorkOrderActionsProps {
  row: IWorkOrderModel;
  onCancel?: (workOrder: IWorkOrderModel) => void;
}

export const WorkOrderActions: React.FC<IWorkOrderActionsProps> = ({ row, onCancel }) => {
  const { navigate } = useTab();

  return (
    <Row>
      <Show
        visible={[WorkOrderStatusName.Submitted, WorkOrderStatusName.InProgress].includes(
          row.status,
        )}
      >
        <FaStop
          className="button button-link red clickable"
          title="Cancel"
          onClick={() => onCancel?.({ ...row, status: WorkOrderStatusName.Cancelled })}
        />
      </Show>
      <Show visible={row.status === WorkOrderStatusName.Completed && !row.content?.isApproved}>
        <FaPen
          className="button button-link completed clickable"
          title="Review"
          onClick={() => navigate(row.contentId ?? 0, '/contents')}
        />
      </Show>
    </Row>
  );
};
