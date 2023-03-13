import { ISourceModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellEllipsis } from 'tno-core';

export const columns: (Column<ISourceModel> &
  UseSortByColumnOptions<ISourceModel> &
  UseFiltersColumnOptions<ISourceModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    sortType: 'string',
    width: 3,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Code',
    accessor: 'code',
    sortType: 'string',
    width: 1,
  },
  {
    Header: 'Licence',
    width: 1,
    accessor: (row) => row.license?.name,
    Cell: ({ value }: any) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Topics',
    accessor: 'useInTopics',
    width: 1,
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
];
