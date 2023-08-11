import { CellCheckbox, CellEllipsis, ISettingModel, ITableHookColumn } from 'tno-core';

export const columns: ITableHookColumn<ISettingModel>[] = [
  {
    label: 'Name',
    name: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    name: 'description',
    width: 5,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Value',
    name: 'value',
    width: 1,
    hAlign: 'center',
    cell: (cell) => cell.original.value,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
