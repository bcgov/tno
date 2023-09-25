import { FaFileInvoice, FaPaperPlane, FaTrash } from 'react-icons/fa';
import { CellDate, CellEllipsis, IReportInstanceModel, ITableHookColumn } from 'tno-core';

export const instanceColumns = (options: {
  onDelete?: (model: IReportInstanceModel) => void;
  onResend?: (model: IReportInstanceModel) => void;
  onPreview?: (model: IReportInstanceModel) => void;
}): ITableHookColumn<IReportInstanceModel>[] => [
  {
    label: 'Published On',
    accessor: 'publishedOn',
    width: 1,
    cell: (cell) => <CellDate value={cell.original.publishedOn} />,
  },
  {
    label: 'Owner',
    accessor: 'owner.username',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.owner?.username ?? ''}</CellEllipsis>,
  },
  {
    label: 'Transaction Id',
    accessor: 'txId',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.response.txId}</CellEllipsis>,
  },
  {
    label: '',
    accessor: '',
    width: '25px',
    cell: (cell) => (
      <FaFileInvoice
        className="btn btn-primary"
        onClick={() => options.onPreview?.(cell.original)}
        title="View Report"
      />
    ),
  },
  {
    label: '',
    accessor: '',
    width: '25px',
    cell: (cell) => (
      <FaPaperPlane
        className="btn btn-primary"
        onClick={() => options.onResend?.(cell.original)}
        title="Resend Report"
      />
    ),
  },
  {
    label: '',
    accessor: '',
    width: '25px',
    cell: (cell) => (
      <FaTrash
        className="btn btn-error"
        onClick={() => options.onDelete?.(cell.original)}
        title="Delete Report Instance"
      />
    ),
  },
];
