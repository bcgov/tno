import { NavigateOptions, useTab } from 'components/tab-control';
import moment from 'moment';
import { FaBug, FaCheckCircle, FaClock, FaPen, FaStop } from 'react-icons/fa';
import { FaCirclePause, FaRegCircleRight } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import {
  CellEllipsis,
  ITableHookColumn,
  IWorkOrderModel,
  Row,
  Show,
  Spinner,
  WorkOrderStatusName,
} from 'tno-core';

export interface IColumnsProps {
  onCancel?: (workOrder: IWorkOrderModel) => void;
}

export const useColumns = ({ onCancel }: IColumnsProps): ITableHookColumn<IWorkOrderModel>[] => {
  const { navigate, options } = useTab();

  return [
    {
      label: 'Headline',
      accessor: 'configuration.headline',
      showSort: false,
      width: 3,
      cell: (cell) => (
        <CellEllipsis>
          <Link
            to={`/contents/${cell.original.contentId}`}
            target={options?.open === NavigateOptions.OnPage ? '_self' : '_blank'}
          >
            {cell.original.configuration.headline
              ? cell.original.configuration.headline
              : `${cell.original.contentId} - Headline missing`}
          </Link>
        </CellEllipsis>
      ),
    },
    {
      label: 'Source',
      accessor: 'content.otherSource',
      width: 1,
    },
    {
      label: 'Media Type',
      accessor: 'content.mediaType',
      width: 1,
    },
    {
      label: 'Requested By',
      accessor: 'requestor.username',
      width: 1,
    },
    {
      label: 'Created',
      accessor: 'createdOn',
      width: '160px',
      hAlign: 'center',
      cell: (cell) =>
        cell.original.createdOn ? moment(cell.original.createdOn).format('MM/DD HH:mm:ss A') : '',
    },
    {
      label: 'Status',
      accessor: 'status',
      width: '100px',
      hAlign: 'center',
      cell: (cell) => {
        if (cell.original.status === WorkOrderStatusName.InProgress)
          return <Spinner size="8px" title="Transcribing" />;
        if (
          cell.original.status === WorkOrderStatusName.Completed &&
          !cell.original.content?.isApproved
        )
          return <FaRegCircleRight className="completed" title="Ready to review" />;
        if (
          cell.original.status === WorkOrderStatusName.Completed &&
          cell.original.content?.isApproved
        )
          return <FaCheckCircle className="completed" title="Completed" />;
        if (cell.original.status === WorkOrderStatusName.Submitted)
          return <FaClock className="submitted" title="Submitted" />;
        if (cell.original.status === WorkOrderStatusName.Failed)
          return <FaBug className="failed" title="Failed" />;
        if (cell.original.status === WorkOrderStatusName.Cancelled)
          return <FaCirclePause className="cancelled" title="Cancelled" />;
        return cell.original.status;
      },
    },
    {
      label: '',
      width: '40px',
      hAlign: 'center',
      cell: (cell) => (
        <Row>
          <Show visible={cell.original.status === WorkOrderStatusName.InProgress}>
            <FaStop
              className="button button-link red"
              title="Cancel"
              onClick={() =>
                onCancel?.({ ...cell.original, status: WorkOrderStatusName.Cancelled })
              }
            />
          </Show>
          <Show
            visible={
              cell.original.status === WorkOrderStatusName.Completed &&
              !cell.original.content?.isApproved
            }
          >
            <FaPen
              className="button button-link completed"
              title="Review"
              onClick={() => navigate(cell.original.contentId ?? 0, '/contents')}
            />
          </Show>
        </Row>
      ),
    },
  ];
};
