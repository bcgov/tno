import { CellCheckbox, CellEllipsis, IMinisterModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<IMinisterModel>[] = [
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Position',
    name: 'position',
    width: 5,
    cell: (cell) => <CellEllipsis>{cell.original.position}</CellEllipsis>,
  },
  // KGM : Hidden for future use with elasticsearch synonyms
  // {
  //   label: 'Aliases',
  //   name: 'aliases',
  //   width: 1,
  //   cell: (cell) => <CellEllipsis>{cell.original.aliases}</CellEllipsis>,
  // },
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
