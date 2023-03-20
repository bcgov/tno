import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellDate, IContentReferenceModel } from 'tno-core';

export const contentReferenceColumns: (Column<IContentReferenceModel> &
  UseSortByColumnOptions<IContentReferenceModel> &
  UseFiltersColumnOptions<IContentReferenceModel>)[] = [
  {
    Header: 'UID',
    accessor: 'uid',
    width: 5,
  },
  {
    Header: 'Source',
    accessor: 'source',
    width: 1,
  },
  {
    Header: 'Published On',
    accessor: 'publishedOn',
    Cell: (cell) => <CellDate value={cell.value} />,
    width: 1,
  },
  {
    Header: 'Updated On',
    accessor: 'sourceUpdatedOn',
    Cell: (cell) => <CellDate value={cell.value} />,
    width: 1,
  },
  {
    Header: 'Status',
    accessor: (row) => row.status,
    width: 1,
  },
];
