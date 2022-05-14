import { IContentReferenceModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Date } from 'tno-core';

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
    Cell: (cell) => <Date value={cell.value} />,
    width: 25,
  },
  {
    Header: 'Updated On',
    accessor: 'sourceUpdatedOn',
    Cell: (cell) => <Date value={cell.value} />,
    width: 25,
  },
  {
    Header: 'Status',
    accessor: (row) => row.workflowStatus,
    width: 25,
  },
];
