import { ContentStatusName, ContentTypeName, IContentModel } from 'hooks/api-editor';
import { FaFeather } from 'react-icons/fa';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Date, Ellipsis, Show } from 'tno-core';
import { formatIdirUsername } from 'utils/formatIdir';

import { getStatusText } from '../utils';

export const columns: (Column<IContentModel> & UseSortByColumnOptions<IContentModel>)[] = [
  {
    id: 'id',
    Header: 'Headline',
    accessor: 'headline',
    width: 5,
    Cell: (cell) => (
      <Ellipsis data-for="main-tooltip" data-tip={cell.value} className="headline">
        <Show
          visible={
            cell.row.original.contentType === ContentTypeName.Snippet && !!cell.row.original.body
          }
        >
          <FaFeather />
        </Show>
        <span>{cell.value}</span>
      </Ellipsis>
    ),
  },
  {
    id: 'otherSource',
    Header: 'Source',
    width: 1,
    accessor: 'otherSource',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'productId',
    Header: 'Designation',
    width: 2,
    accessor: (row) => row.product?.name,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'page',
    Header: 'Section Page',
    width: 1,
    accessor: (row) =>
      row.printContent?.section ? `${row.printContent.section}/${row.page}` : row.page,
    Cell: ({ value }: { value: string }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    id: 'ownerId',
    Header: 'Username',
    width: 1,
    accessor: (row) => row.owner?.displayName,
    Cell: ({ value }: { value: string }) => <Ellipsis>{formatIdirUsername(value)}</Ellipsis>,
  },
  {
    id: 'status',
    Header: 'Status',
    width: 1,
    accessor: (row) => <div className="center">{getStatusText(row.status)}</div>,
  },
  {
    id: 'publishedOn',
    Header: 'Pub Date',
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
    Header: 'Use',
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
