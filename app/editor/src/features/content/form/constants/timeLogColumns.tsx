import { faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Column } from 'react-table';

import { ITimeTrackingForm } from '../components/time-log/interfaces';
import { IContentForm } from '../interfaces';

const colWidth = window.innerWidth > 1500 ? 140 : window.innerWidth > 1450 ? 100 : 80;
/** columns located within file for state manipulation */
export const timeLogColumns = (
  setFieldValue: Function,
  values: IContentForm,
): Column<ITimeTrackingForm>[] => [
  {
    id: 'effort',
    maxWidth: colWidth,
    Header: () => <div className="center">TIME</div>,
    accessor: 'effort',
    Cell: ({ value }) => <div className="center">{value} Min</div>,
  },
  {
    id: 'user',
    maxWidth: colWidth,
    Header: () => <div className="center">USER</div>,
    accessor: 'userName',
    Cell: ({ value }) => <div className="center">{value}</div>,
  },
  {
    id: 'activity',
    maxWidth: colWidth,
    Header: () => <div className="center">TASK</div>,
    accessor: 'activity',
    Cell: ({ value }) => <div className="center">{value}</div>,
  },
  {
    id: 'content',
    maxWidth: colWidth,
    Header: () => <div className="center">DATE</div>,
    accessor: 'createdOn',
    Cell: ({ value }: any) => {
      const date = new Date(value);
      return (
        <div className="center">{`${
          date.getMonth() + 1
        }/${date.getDate()}/${date.getFullYear()}`}</div>
      );
    },
  },
  {
    id: 'actions',
    maxWidth: colWidth,
    Header: () => <div className="center">ACTION</div>,
    Cell: ({ row, data }: any) => (
      <div className="center">
        <FontAwesomeIcon
          onClick={() => {
            values.timeTrackings?.splice(row.id, 1);
            setFieldValue('timeTrackings', values.timeTrackings);
          }}
          icon={faTrash}
        />
      </div>
    ),
  },
];
