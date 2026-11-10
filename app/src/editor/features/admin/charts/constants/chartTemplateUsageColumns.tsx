import { CellCheckbox, CellEllipsis, type ITableHookColumn } from 'tno-core';

import { type IChartTemplateUsageModel } from '../interfaces';

/**
 * Columns for the reports that use a chart template.
 * @returns Columns
 */
export const getChartTemplateUsageColumns = (): Array<
  ITableHookColumn<IChartTemplateUsageModel>
> => [
  {
    label: 'Report',
    accessor: 'name',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Sections',
    accessor: 'sections',
    width: 4,
    cell: (cell) => <CellEllipsis>{cell.original.sections.join(', ')}</CellEllipsis>,
  },
  {
    label: 'Owner',
    accessor: 'owner',
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.owner}</CellEllipsis>,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
