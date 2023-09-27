import { CellCheckbox, CellEllipsis, IReportTemplateModel, ITableHookColumn } from 'tno-core';

export const reportTemplateColumns: ITableHookColumn<IReportTemplateModel>[] = [
  {
    label: 'Name',
    accessor: 'name',
    width: 2,
    cell: (cell) => <CellEllipsis>{cell.original.name}</CellEllipsis>,
  },
  {
    label: 'Description',
    accessor: 'description',
    width: 5,
    cell: (cell) => <CellEllipsis>{cell.original.description}</CellEllipsis>,
  },
  {
    label: 'Report Type',
    accessor: 'reportType',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellEllipsis>{cell.original.reportType}</CellEllipsis>,
  },
  {
    label: 'Enabled',
    accessor: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
