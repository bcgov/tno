import { FaPen, FaStop } from 'react-icons/fa6';

import { useTab } from '@/editor/components/tab-control';
import { Row, Show, WorkOrderStatusName, type IWorkOrderModel } from '@/shared';

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
          onClick={() => {
            navigate(row.contentId ?? 0, '/contents');
          }}
        />
      </Show>
    </Row>
  );
};
