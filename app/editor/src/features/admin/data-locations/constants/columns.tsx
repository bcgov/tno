import { IDataLocationModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellEllipsis } from 'tno-core';

export const columns: (Column<IDataLocationModel> &
  UseSortByColumnOptions<IDataLocationModel> &
  UseFiltersColumnOptions<IDataLocationModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Description',
    accessor: 'description',
    width: 5,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
];
