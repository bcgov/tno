import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellEllipsis, IReportModel } from 'tno-core';

export const columns: (Column<IReportModel> &
  UseSortByColumnOptions<IReportModel> &
  UseFiltersColumnOptions<IReportModel>)[] = [
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
    Header: 'Public',
    accessor: 'isPublic',
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
