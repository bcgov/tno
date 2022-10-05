import { ContentStatusName, IContentModel } from 'hooks/api-editor';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis } from 'tno-core/dist/components/cell';
import { formatIdirUsername } from 'utils';

export const condensedColumns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
  {
    id: 'id',
    Header: () => <div className="center">Headline</div>,
    accessor: 'headline',
    width: 4,
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'otherSource',
    Header: () => <div className="center">Source</div>,
    accessor: 'otherSource',
    width: 1,
    Cell: ({ value }) => (
      <Ellipsis>
        <div className="center">{value}</div>
      </Ellipsis>
    ),
  },
  {
    id: 'productId',
    Header: () => <div className="center">Designation</div>,
    width: 2,
    accessor: (row) => row.product?.name,
    Cell: ({ value }: { value: string }) => (
      <Ellipsis>
        <div className="center">{value}</div>
      </Ellipsis>
    ),
  },
  {
    id: 'page',
    Header: () => <div className="center">Page</div>,
    width: 1,
    accessor: (row) => row.page,
    Cell: ({ value }: { value: string }) => <div className="center">{value}</div>,
  },
  {
    id: 'owner',
    Header: () => <div className="center">Username</div>,
    width: 1,
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
    width: 1,
    accessor: (row) => row.status,
    Cell: ({ value }: { value: string }) => <div className="center">{value}</div>,
  },
  {
    id: 'publishedOn',
    Header: () => <div className="center">Pub Date</div>,
    width: 1,
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
    width: 1,
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
