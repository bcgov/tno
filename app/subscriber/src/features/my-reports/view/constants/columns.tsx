import { FaFileInvoice } from 'react-icons/fa';
import { CellDate, CellEllipsis, IReportInstanceModel, ITableHookColumn } from 'tno-core';

export const columns = (options: {
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
    width: '1',
    cell: (cell) => (
      <FaFileInvoice
        className="btn btn-primary"
        onClick={() => options.onPreview?.(cell.original)}
        title="View Report"
      />
    ),
  },
];
