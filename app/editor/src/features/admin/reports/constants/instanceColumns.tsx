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
    label: 'Sent On',
    accessor: 'sentOn',
    width: 1,
    cell: (cell) => <CellDate value={cell.original.sentOn} />,
  },
  {
    label: 'Owner',
    accessor: 'owner.username',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.owner?.username ?? ''}</CellEllipsis>,
  },
  {
    label: 'Email Status',
    accessor: 'status',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.status}</CellEllipsis>,
  },
  {
    label: '',
    accessor: '',
    width: '25px',
    cell: (cell) => (
      <FaFileInvoice
        className="button button-primary"
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
        className="button button-primary"
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
        className="button button-error"
        onClick={() => options.onDelete?.(cell.original)}
        title="Delete Report Instance"
      />
    ),
  },
];
