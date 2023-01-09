import { IScheduleModel } from 'hooks/api-editor';
import { Column, UseFiltersColumnOptions, UseSortByColumnOptions } from 'react-table';
import { CellCheckbox, CellEllipsis } from 'tno-core';

import { weekDayNameAbbrev } from '../utils';

export const columns: (Column<IScheduleModel> &
  UseSortByColumnOptions<IScheduleModel> &
  UseFiltersColumnOptions<IScheduleModel>)[] = [
  {
    id: 'id',
    Header: 'Name',
    accessor: 'name',
    Cell: ({ value }) => <CellEllipsis>{value}</CellEllipsis>,
  },
  {
    Header: 'Start/Stop',
    accessor: (row) => `${row.startAt ? row.startAt : ''}${row.stopAt ? `-${row.stopAt}` : ''}`,
  },
  {
    Header: 'Enabled',
    accessor: 'isEnabled',
    Cell: (cell) => <CellCheckbox checked={cell.value} />,
  },
  {
    Header: 'Days of the Week',
    accessor: 'runOnWeekDays',
    Cell: (cell) => <>{weekDayNameAbbrev(cell.value)}</>,
  },
];
