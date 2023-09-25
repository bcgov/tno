import moment from 'moment';
import { CellEllipsis, ITableHookColumn, IWorkOrderModel } from 'tno-core';

export const getColumns = (
  onClickOpen?: (contentId: number) => void,
): ITableHookColumn<IWorkOrderModel>[] => [
  {
    label: 'Type',
    accessor: 'workType',
    width: 2,
    cell: (cell) => <span>{cell.original.workType.replace(/([A-Z])/g, ' $1')}</span>,
  },
  {
    label: 'Content',
    accessor: 'configuration',
    width: 4,
    cell: (cell) => (
      <CellEllipsis
        className="link"
        onClick={(e) => {
          if (cell.original.configuration.contentId) {
            e.preventDefault();
            e.stopPropagation();
            onClickOpen?.(cell.original.configuration.contentId);
          }
        }}
      >
        {cell.original.configuration.headline ?? cell.original.configuration.contentId}
      </CellEllipsis>
    ),
  },
  {
    label: 'Submitted',
    accessor: 'createdOn',
    width: 2,
    cell: (cell) => {
      const created = moment(cell.original.createdOn);
      const text = created.isValid() ? created.format('MM/DD/YYYY HH:mm:ss') : '';
      return <div className="center">{text}</div>;
    },
  },
  {
    label: 'Updated',
    accessor: 'updatedOn',
    width: 2,
    cell: (cell) => {
      const created = moment(cell.original.updatedOn);
      const text = created.isValid() ? created.format('MM/DD/YYYY HH:mm:ss') : '';
      return <div className="center">{text}</div>;
    },
  },
  {
    label: 'Status',
    accessor: 'status',
    width: 1,
    cell: (cell) => <span>{cell.original.status.replace(/([A-Z])/g, ' $1')}</span>,
  },
];
