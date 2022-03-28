import { Checkbox, Ellipsis } from 'components/cell';
import { IScheduleModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';

export const columns: (Column<IScheduleModel> &
  UseSortByColumnOptions<IScheduleModel> &
  UseFiltersColumnOptions<IScheduleModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    Cell: ({ value }) => <Ellipsis>{value}</Ellipsis>,
  },
  {
    Header: 'Start/Stop',
    accessor: (row) => `${row.startAt}-${row.stopAt}`,
  },
  {
    Header: 'Enabled',
    accessor: 'enabled',
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
  {
    Header: 'Days of the Week',
    accessor: 'runOnWeekDays',
  },
];
