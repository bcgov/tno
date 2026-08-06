import moment from 'moment';
import { CellCheckbox, CellEllipsis, type ITableHookColumn } from 'tno-core';

import { type IAutomationProfileModel, type IAutomationRunModel } from '../interfaces';

const formatRunDate = (value?: string): string =>
  value ? moment(value).format('YYYY-MM-DD HH:mm:ss') : '-';

const getStepCount = (profile: IAutomationProfileModel): number => {
  const legacyProfile = profile as IAutomationProfileModel & { rules?: unknown[] };
  return legacyProfile.steps?.length ?? legacyProfile.rules?.length ?? 0;
};

export const columns: Array<ITableHookColumn<IAutomationProfileModel>> = [
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Schedule',
    accessor: 'schedules',
    width: 1,
    cell: (cell) => {
      const enabled = (cell.original.schedules ?? []).filter((schedule) => schedule.isEnabled);
      return (
        <CellEllipsis>
          {enabled.length === 0
            ? 'Manual only'
            : enabled.length === 1
            ? `Daily at ${(enabled[0].startAt ?? '').slice(0, 5) || 'any time'}`
            : `${enabled.length} schedules`}
        </CellEllipsis>
      );
    },
  },
  {
    label: 'Steps',
    accessor: 'steps.length',
    width: 1,
    hAlign: 'center',
    cell: (cell) => getStepCount(cell.original),
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];

export const runColumns: Array<ITableHookColumn<IAutomationRunModel>> = [
  {
    label: 'Started',
    accessor: 'startedOn',
    width: 1,
    cell: (cell) => <CellEllipsis>{formatRunDate(cell.original.startedOn)}</CellEllipsis>,
  },
  {
    label: 'Completed',
    accessor: 'completedOn',
    width: 1,
    cell: (cell) => <CellEllipsis>{formatRunDate(cell.original.completedOn)}</CellEllipsis>,
  },
  {
    label: 'Status',
    accessor: 'status',
    width: 1,
    cell: (cell) => <CellEllipsis>{`${cell.original.status ?? '-'}`}</CellEllipsis>,
  },
  {
    label: 'Trigger',
    accessor: 'trigger',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.trigger ?? '-'}</CellEllipsis>,
  },
  {
    label: 'Note',
    accessor: 'note',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.note ?? ''}</CellEllipsis>,
  },
];
