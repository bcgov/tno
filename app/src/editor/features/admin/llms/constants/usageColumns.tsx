import { CellCheckbox, CellEllipsis, type ITableHookColumn } from 'tno-core';

import { type ILLMUsageItemModel } from '../interfaces';

const usageTypeLabels: Record<ILLMUsageItemModel['type'], string> = {
  ReportSection: 'Report',
  AutomationProfile: 'Automation',
  ContentAnalysis: 'AI Analysis',
  Search: 'Search',
};

/**
 * Columns for the items that use an LLM connection.
 * @returns Columns
 */
export const getUsageColumns = (): Array<ITableHookColumn<ILLMUsageItemModel>> => [
  {
    label: 'Type',
    accessor: 'type',
    width: 1,
    cell: (cell) => <CellEllipsis>{usageTypeLabels[cell.original.type]}</CellEllipsis>,
  },
  {
    label: 'Name',
    accessor: 'name',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Used In',
    accessor: 'detail',
    width: 4,
    cell: (cell) => <CellEllipsis>{cell.original.detail}</CellEllipsis>,
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
