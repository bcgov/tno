import { faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IContentModel, ITimeTrackingModel } from 'hooks/api-editor';
import { Column } from 'react-table';
import { Center } from 'tno-core';

import { getTotalTime } from '../utils';

/** columns located within file for state manipulation */
export const timeLogColumns = (
  setTotalEffort: Function,
  setFieldValue: Function,
  values: IContentModel,
): Column<ITimeTrackingModel>[] => [
  {
    id: 'effort',
    Header: () => <Center>TIME</Center>,
    accessor: 'effort',
    Cell: ({ value }) => <Center>{value}</Center>,
  },
  {
    id: 'user',
    Header: () => <Center>USER</Center>,
    accessor: 'userName',
    Cell: ({ value }) => <Center>{value}</Center>,
  },
  {
    id: 'activity',
    Header: () => <Center>TASK</Center>,
    accessor: 'activity',
    Cell: ({ value }) => <Center>{value}</Center>,
  },
  {
    id: 'content',
    Header: () => <Center>DATE</Center>,
    accessor: 'createdOn',
    Cell: ({ value }: any) => {
      const date = new Date(value);
      return <Center>{`${date.getMonth()}/${date.getDate()}/${date.getFullYear()}`}</Center>;
    },
  },
  {
    id: 'actions',
    Header: () => <Center>ACTION</Center>,
    Cell: ({ row, data }: any) => (
      <Center>
        <FontAwesomeIcon
          onClick={() => {
            values.timeTrackings?.splice(row.id, 1);
            setFieldValue('timeTrackings', values.timeTrackings);
            setTotalEffort(!!values.timeTrackings ? getTotalTime(values.timeTrackings) : 0);
          }}
          icon={faTrash}
        />
      </Center>
    ),
  },
];
