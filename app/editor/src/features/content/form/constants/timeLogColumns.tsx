import { ITimeTrackingModel } from 'hooks/api-editor';
import { Column } from 'react-table';

export const timeLogColumns: Column<ITimeTrackingModel>[] = [
  {
    id: 'effort',
    Header: 'TIME',
    accessor: 'effort',
  },
  {
    id: 'user',
    Header: 'USER',
    accessor: 'userName',
  },
  {
    id: 'activity',
    Header: 'TASK',
    accessor: 'activity',
  },
  {
    id: 'content',
    Header: 'DATE',
    accessor: 'createdOn',
    Cell: ({ value }: any) => {
      const date = new Date(value);
      return <>{`${date.getMonth()}/${date.getDate()}/${date.getFullYear()}`}</>;
    },
  },
];
