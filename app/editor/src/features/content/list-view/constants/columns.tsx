import { ContentStatusName, IContentModel } from 'hooks/api-editor';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';
import { formatIdirUsername } from 'utils';

export const columns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
  {
    id: 'headline',
    Header: 'Headline',
    accessor: 'headline',
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
    maxWidth: 100,
    accessor: (row) => row.mediaType?.name,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'section',
    Header: 'Section/Page',
    maxWidth: 125,
    accessor: (row) =>
      row.printContent?.section ? `${row.printContent.section}/${row.page}` : row.page,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'ownerId',
    Header: 'Username',
    maxWidth: 125,
    accessor: (row) => row.owner?.displayName,
    Cell: ({ value }: { value: string }) => <Ellipsis>{formatIdirUsername(value)}</Ellipsis>,
  },
  {
    id: 'status',
    Header: 'Status',
    maxWidth: 100,
    accessor: (row) => row.status,
  },
  {
    id: 'publishedOn',
    Header: 'Date',
    maxWidth: 100,
    accessor: (row) => row.publishedOn ?? row.createdOn,
    Cell: ({ value }: any) => <Date value={value} />,
  },
  {
    id: 'use',
    Header: 'Use',
    disableSortBy: true,
    maxWidth: 50,
    accessor: (row) =>
      row.status === ContentStatusName.Publish || row.status === ContentStatusName.Published,
    Cell: ({ value }: { value: boolean }) => {
      return <Checkbox checked={value} />;
    },
  },
];
