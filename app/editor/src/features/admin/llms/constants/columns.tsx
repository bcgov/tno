import { CellCheckbox, CellEllipsis, type ILLMModel, type ITableHookColumn } from 'tno-core';

export const columns: Array<ITableHookColumn<ILLMModel>> = [
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Deployment Name',
    accessor: 'deploymentName',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.deploymentName}</CellEllipsis>,
  },
  {
    label: 'Agent',
    accessor: 'agentName',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.agentName}</CellEllipsis>,
  },
  {
    label: 'Public',
    accessor: 'isPublic',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={!!cell.original.isPublic} />,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
