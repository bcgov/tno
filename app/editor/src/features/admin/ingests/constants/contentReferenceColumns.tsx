import { IContentReferenceModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellDate } from 'tno-core';

export const contentReferenceColumns: (Column<IContentReferenceModel> &
  UseSortByColumnOptions<IContentReferenceModel> &
  UseFiltersColumnOptions<IContentReferenceModel>)[] = [
  {
    Header: 'UID',
    accessor: 'uid',
  },
  {
    Header: 'Published On',
    accessor: 'publishedOn',
    Cell: (cell) => <CellDate value={cell.value} />,
    width: 25,
  },
  {
    Header: 'Updated On',
    accessor: 'sourceUpdatedOn',
    Cell: (cell) => <CellDate value={cell.value} />,
    width: 25,
  },
  {
    Header: 'Status',
    accessor: (row) => row.status,
    width: 25,
  },
];
