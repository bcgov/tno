import { CellCheckbox, CellEllipsis, IReportTemplateModel, ITableHookColumn } from 'tno-core';

export const reportTemplateColumns: ITableHookColumn<IReportTemplateModel>[] = [
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
    label: 'Report Type',
    name: 'reportType',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellEllipsis>{cell.original.reportType}</CellEllipsis>,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
