import { faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ITimeTrackingModel } from 'hooks/api-editor';
import { Column } from 'react-table';
import { Center } from 'tno-core';

export const timeLogColumns: Column<ITimeTrackingModel>[] = [
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
            data.splice(row.id, 1);
          }}
          icon={faTrash}
        />
      </Center>
    ),
  },
];
