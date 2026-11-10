import { CellCheckbox, CellEllipsis, type IScheduleModel, type ITableHookColumn } from '@/shared';

import { weekDayNameAbbrev } from '../utils';

export const columns: Array<ITableHookColumn<IScheduleModel>> = [
  {
    label: 'Name',
    accessor: 'name',
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Start/Stop',
    accessor: (row) => `${row.startAt ? row.startAt : ''}${row.stopAt ? `-${row.stopAt}` : ''}`,
    cell: (cell) => (
      <>{`${cell.original.startAt ? cell.original.startAt : ''}${
        cell.original.stopAt ? `-${cell.original.stopAt}` : ''
      }`}</>
    ),
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
  {
    label: 'Days of the Week',
    accessor: 'runOnWeekDays',
    cell: (cell) => <>{weekDayNameAbbrev(cell.original.runOnWeekDays)}</>,
  },
];
