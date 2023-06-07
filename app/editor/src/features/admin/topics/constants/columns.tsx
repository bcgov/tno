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
    width: 1,
    cell: (cell) => <CellEllipsis>{cell.original.topicType}</CellEllipsis>,
  },
  {
    label: 'Order',
    name: 'sortOrder',
    width: 1,
    hAlign: 'center',
    cell: (cell) => cell.original.sortOrder,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
