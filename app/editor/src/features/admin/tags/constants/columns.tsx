import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellEllipsis, ITagModel } from 'tno-core';

export const columns: (Column<ITagModel> &
  UseSortByColumnOptions<ITagModel> &
  UseFiltersColumnOptions<ITagModel>)[] = [
  {
    id: 'code',
    Header: 'Code',
    accessor: 'code',
    width: 1,
  },
  {
    id: 'name',
    Header: 'Name',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    id: 'description',
    Header: 'Description',
    width: 5,
    accessor: 'description',
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    id: 'isEnabled',
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
];
