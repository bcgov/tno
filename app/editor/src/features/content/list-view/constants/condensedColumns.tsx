import { ContentStatusName, IContentModel } from 'hooks/api-editor';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Center } from 'tno-core';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';
import { formatIdirUsername } from 'utils';

export const condensedColumns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
  {
    id: 'headline',
    Header: () => <Center>Headline</Center>,
    accessor: 'headline',
    width: 180,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'source',
    Header: () => <Center>Source</Center>,
    accessor: 'source',
    width: 100,
    Cell: ({ value }) => (
      <Ellipsis>
        <Center>{value}</Center>
      </Ellipsis>
    ),
  },
  {
    id: 'mediaType',
    Header: () => <Center>Type</Center>,
    width: 100,
    accessor: (row) => row.mediaType?.name,
    Cell: ({ value }: { value: string }) => (
      <Ellipsis>
        <Center>{value}</Center>
      </Ellipsis>
    ),
  },
  {
    id: 'page',
    Header: () => <Center>Page</Center>,
    width: 70,
    accessor: (row) => row.page,
    Cell: ({ value }: { value: string }) => <Center>{value}</Center>,
  },
  {
    id: 'ownerId',
    Header: () => <Center>Username</Center>,
    width: 100,
    accessor: (row) => row.owner?.displayName,
    Cell: ({ value }: { value: string }) => (
      <Ellipsis>
        <Center>{formatIdirUsername(value)}</Center>
      </Ellipsis>
    ),
  },
  {
    id: 'status',
    Header: () => <Center>Status</Center>,
    width: 100,
    accessor: (row) => row.status,
    Cell: ({ value }: { value: string }) => <Center>{value}</Center>,
  },
  {
    id: 'createdOn',
    Header: () => <Center>Date</Center>,
    width: 100,
    accessor: (row) => row.createdOn,
    Cell: ({ value }: any) => (
      <Center>
        <Date value={value} />
      </Center>
    ),
  },
  {
    id: 'use',
    Header: () => <Center>Use</Center>,
    disableSortBy: true,
    width: 50,
    accessor: (row) =>
      row.status === ContentStatusName.Publish || row.status === ContentStatusName.Published,
    Cell: ({ value }: { value: boolean }) => {
      return (
        <Center>
          <Checkbox checked={value} />
        </Center>
      );
    },
  },
];
