import { IScheduleModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { Checkbox, Ellipsis } from 'tno-core/dist/components/cell';

import { weekDayNameAbbrev } from '../utils';

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
    accessor: (row) => `${row.startAt ? row.startAt : ''}${row.stopAt ? `-${row.stopAt}` : ''}`,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    Cell: (cell) => <Checkbox checked={cell.value} />,
  },
  {
    Header: 'Days of the Week',
    accessor: 'runOnWeekDays',
    Cell: (cell) => weekDayNameAbbrev(cell.value),
  },
];
