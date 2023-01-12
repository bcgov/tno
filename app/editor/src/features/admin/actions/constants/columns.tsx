import { IActionModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellEllipsis } from 'tno-core';

export const columns: (Column<IActionModel> &
  UseSortByColumnOptions<IActionModel> &
  UseFiltersColumnOptions<IActionModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    width: 2,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Description',
    accessor: 'description',
    width: 7,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
];
