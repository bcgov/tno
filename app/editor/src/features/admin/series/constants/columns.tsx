import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellEllipsis, ISeriesModel } from 'tno-core';

export const columns: (Column<ISeriesModel> &
  UseSortByColumnOptions<ISeriesModel> &
  UseFiltersColumnOptions<ISeriesModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    width: 3,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Source',
    width: 1,
    accessor: (row) => row.source?.code,
    Cell: ({ value }: any) => value,
  },
  {
    Header: 'Description',
    accessor: 'description',
    width: 4,
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
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
