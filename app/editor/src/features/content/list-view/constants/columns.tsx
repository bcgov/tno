import { ContentStatusName, IContentModel } from 'hooks/api-editor';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';
import { formatIdirUsername } from 'utils';

export const columns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
  {
    id: 'headline',
    Header: 'Headline',
    accessor: 'headline',
    width: 300,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'source',
    Header: 'Source',
    accessor: 'source',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'mediaType',
    Header: 'Type',
    width: 125,
    accessor: (row) => row.mediaType?.name,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'section',
    Header: 'Section/Page',
    width: 125,
    accessor: (row) =>
      row.printContent?.section ? `${row.printContent.section}/${row.page}` : row.page,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'ownerId',
    Header: 'Username',
    width: 125,
    accessor: (row) => row.owner?.displayName,
    Cell: ({ value }: { value: string }) => <Ellipsis>{formatIdirUsername(value)}</Ellipsis>,
  },
  {
    id: 'status',
    Header: 'Status',
    width: 100,
    accessor: (row) => row.status,
  },
  {
    id: 'createdOn',
    Header: 'Date',
    width: 100,
    accessor: (row) => row.createdOn,
    Cell: ({ value }: any) => <Date value={value} />,
  },
  {
    id: 'use',
    Header: 'Use',
    disableSortBy: true,
    width: 50,
    accessor: (row) =>
      row.status === ContentStatusName.Publish || row.status === ContentStatusName.Published,
    Cell: ({ value }: { value: boolean }) => {
      return <Checkbox checked={value} />;
    },
  },
];
