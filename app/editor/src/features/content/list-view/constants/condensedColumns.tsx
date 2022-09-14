import { ContentStatusName, IContentModel } from 'hooks/api-editor';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';
import { formatIdirUsername } from 'utils';

export const condensedColumns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
  {
    id: 'headline',
    Header: () => <div className="center">Headline</div>,
    accessor: 'headline',
    width: 180,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'source',
    Header: () => <div className="center">Source</div>,
    accessor: 'source',
    width: 100,
    Cell: ({ value }) => (
      <Ellipsis>
        <div className="center">{value}</div>
      </Ellipsis>
    ),
  },
  {
    id: 'mediaType',
    Header: () => <div className="center">Type</div>,
    width: 100,
    accessor: (row) => row.mediaType?.name,
    Cell: ({ value }: { value: string }) => (
      <Ellipsis>
        <div className="center">{value}</div>
      </Ellipsis>
    ),
  },
  {
    id: 'page',
    Header: () => <div className="center">Page</div>,
    width: 70,
    accessor: (row) => row.page,
    Cell: ({ value }: { value: string }) => <div className="center">{value}</div>,
  },
  {
    id: 'ownerId',
    Header: () => <div className="center">Username</div>,
    width: 100,
    accessor: (row) => row.owner?.displayName,
    Cell: ({ value }: { value: string }) => (
      <Ellipsis>
        <div className="center">{formatIdirUsername(value)}</div>
      </Ellipsis>
    ),
  },
  {
    id: 'status',
    Header: () => <div className="center">Status</div>,
    width: 100,
    accessor: (row) => row.status,
    Cell: ({ value }: { value: string }) => <div className="center">{value}</div>,
  },
  {
    id: 'publishedOn',
    Header: () => <div className="center">Pub Date</div>,
    width: 100,
    accessor: (row) => row.publishedOn ?? row.createdOn,
    Cell: ({ value }: any) => (
      <div className="center">
        <Date value={value} />
      </div>
    ),
  },
  {
    id: 'use',
    Header: () => <div className="center">Use</div>,
    disableSortBy: true,
    width: 50,
    accessor: (row) =>
      row.status === ContentStatusName.Publish || row.status === ContentStatusName.Published,
    Cell: ({ value }: { value: boolean }) => {
      return (
        <div className="center">
          <Checkbox checked={value} />
        </div>
      );
    },
  },
];
