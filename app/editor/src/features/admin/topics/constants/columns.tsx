import { CellCheckbox, CellEllipsis, ITableHookColumn, ITopicModel } from 'tno-core';

export const columns: ITableHookColumn<ITopicModel>[] = [
  {
    label: 'Name',
    name: 'name',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    name: 'description',
    width: 3,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Type',
    name: 'topicType',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.topicType}</CellEllipsis>,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
