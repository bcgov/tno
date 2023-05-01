import moment from 'moment';
import { CellEllipsis, ITableHookColumn, IWorkOrderModel } from 'tno-core';

export const getColumns = (
  onClickOpen?: (contentId: number) => void,
): ITableHookColumn<IWorkOrderModel>[] => [
  {
    label: 'Type',
    name: 'workType',
    width: 2,
    cell: (cell) => <span>{cell.original.workType.replace(/([A-Z])/g, ' $1')}</span>,
  },
  {
    label: 'Content',
    name: 'configuration',
    width: 4,
    cell: (cell) => <CellEllipsis>{JSON.stringify(cell.original.configuration)}</CellEllipsis>,
  },
  {
    label: 'Submitted',
    name: 'createdOn',
    width: 2,
    cell: (cell) => {
      const created = moment(cell.original.createdOn);
      const text = created.isValid() ? created.format('MM/DD/YYYY HH:mm:ss') : '';
      return <div className="center">{text}</div>;
    },
  },
  {
    label: 'Updated',
    name: 'updatedOn',
    width: 2,
    cell: (cell) => {
      const created = moment(cell.original.updatedOn);
      const text = created.isValid() ? created.format('MM/DD/YYYY HH:mm:ss') : '';
      return <div className="center">{text}</div>;
    },
  },
  {
    label: 'Status',
    name: 'status',
    width: 1,
    cell: (cell) => <span>{cell.original.status.replace(/([A-Z])/g, ' $1')}</span>,
  },
];
