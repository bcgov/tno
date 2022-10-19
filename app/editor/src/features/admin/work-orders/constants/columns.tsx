import { IWorkOrderModel } from 'hooks/api-editor';
import moment from 'moment';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Ellipsis } from 'tno-core/dist/components/cell';

export const columns: (Column<IWorkOrderModel> &
  UseSortByColumnOptions<IWorkOrderModel> &
  UseFiltersColumnOptions<IWorkOrderModel>)[] = [
  {
    id: 'id',
    Header: 'Type',
    accessor: 'workType',
    width: 2,
    Cell: ({ value }) => <span>{value.replace(/([A-Z])/g, ' $1')}</span>,
  },
  {
    Header: 'Content',
    accessor: 'content',
    width: 4,
    Cell: ({ value }) => <Ellipsis>{value?.headline}</Ellipsis>,
  },
  {
    Header: 'Submitted',
    accessor: 'createdOn',
    width: 2,
    Cell: ({ value }) => {
      const created = moment(value);
      const text = created.isValid() ? created.format('MM/DD/YYYY HH:mm:ss') : '';
      return <div className="center">{text}</div>;
    },
  },
  {
    Header: 'Updated',
    accessor: 'updatedOn',
    width: 2,
    Cell: ({ value }) => {
      const created = moment(value);
      const text = created.isValid() ? created.format('MM/DD/YYYY HH:mm:ss') : '';
      return <div className="center">{text}</div>;
    },
  },
  {
    Header: 'Status',
    accessor: 'status',
    width: 1,
    Cell: ({ value }) => <span>{value.replace(/([A-Z])/g, ' $1')}</span>,
  },
];
