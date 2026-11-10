import { FaTrash } from 'react-icons/fa';

import { type ITableHookColumn } from '@/shared';

import { type ITimeTrackingForm } from '../components/time-log/interfaces';
import { type IContentForm } from '../interfaces';

/** columns located within file for state manipulation */
export const timeLogColumns = (
  setFieldValue: Function,
  values: IContentForm,
): Array<ITableHookColumn<ITimeTrackingForm>> => [
  {
    label: 'TIME',
    accessor: 'effort',
    hAlign: 'center',
    cell: (cell) => <>{cell.original.effort} Min</>,
  },
  {
    label: 'USER',
    accessor: 'userName',
    hAlign: 'center',
    cell: (cell) => <>{cell.original.userName}</>,
  },
  {
    label: 'TASK',
    accessor: 'activity',
    hAlign: 'center',
    cell: (cell) => <>{cell.original.activity}</>,
  },
  {
    label: 'DATE',
    accessor: 'createdOn',
    hAlign: 'center',
    cell: (cell) => {
      const date = new Date(cell.original.createdOn ?? '');
      return <>{`${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()}`}</>;
    },
  },
  {
    label: 'ACTION',
    hAlign: 'center',
    cell: (cell) => (
      <FaTrash
        className="fa-trash"
        onClick={() => {
          values.timeTrackings?.splice(cell.original.index, 1);
          setFieldValue('timeTrackings', values.timeTrackings);
        }}
      />
    ),
  },
];
