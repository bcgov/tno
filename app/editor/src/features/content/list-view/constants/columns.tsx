import { ContentStatusName, IContentModel } from 'hooks/api-editor';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';

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
    accessor: (row) => row.mediaType?.name,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'section',
    Header: 'Section/Page',
    accessor: (row) =>
      row.printContent?.section ? `${row.printContent.section}/${row.page}` : row.page,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'ownerId',
    Header: 'Username',
    accessor: (row) => row.owner?.displayName,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'status',
    Header: 'Status',
    accessor: (row) => row.status,
  },
  {
    id: 'createdOn',
    Header: 'Date',
    accessor: (row) => row.createdOn,
    Cell: ({ value }: any) => <Date value={value} />,
  },
  {
    id: 'use',
    Header: 'Use',
    disableSortBy: true,
    accessor: (row) =>
      row.status === ContentStatusName.Publish || row.status === ContentStatusName.Published,
    Cell: ({ value }: { value: boolean }) => {
      return <Checkbox checked={value} />;
    },
  },
];
