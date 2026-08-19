import moment from 'moment';
import { FaTrash } from 'react-icons/fa';
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

/**
 * Run history columns. The delete column is only included when 'onDelete' is supplied; its click is
 * stopped from bubbling so removing a run does not also open the run detail modal (row click).
 */
export const getRunColumns = (
  onDelete?: (run: IAutomationRunModel) => void,
): Array<ITableHookColumn<IAutomationRunModel>> => [
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
    cell: (cell) => (
      <CellEllipsis>
        {`${cell.original.trigger ?? '-'}${cell.original.isDryRun ? ' • DRY' : ''}`}
      </CellEllipsis>
    ),
  },
  {
    label: 'Note',
    accessor: 'note',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.note ?? ''}</CellEllipsis>,
  },
  ...(onDelete
    ? [
        {
          label: '',
          accessor: 'id',
          width: 0.4,
          hAlign: 'center',
          showSort: false,
          cell: (cell) => (
            <button
              type="button"
              className="rule-icon-button delete"
              aria-label={`Delete run ${cell.original.id}`}
              title={`Delete run ${cell.original.id}`}
              onClick={(event) => {
                event.stopPropagation();
                onDelete(cell.original);
              }}
            >
              <FaTrash />
            </button>
          ),
        } as ITableHookColumn<IAutomationRunModel>,
      ]
    : []),
];
