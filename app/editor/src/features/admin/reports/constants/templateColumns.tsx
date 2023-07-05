import { CellCheckbox, CellEllipsis, IReportTemplateModel, ITableHookColumn } from 'tno-core';

export const templateColumns: ITableHookColumn<IReportTemplateModel>[] = [
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
    label: 'Sections',
    name: 'enableSections',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.enableSections} />,
  },
  {
    label: 'Section Summary',
    name: 'enableSectionSummary',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.enableSectionSummary} />,
  },
  {
    label: 'Summary',
    name: 'enableSummary',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.enableSummary} />,
  },
  {
    label: 'Charts',
    name: 'enableCharts',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.enableCharts} />,
  },
  {
    label: 'Charts Over Time',
    name: 'enableChartsOverTime',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.enableChartsOverTime} />,
  },
  {
    label: 'Enabled',
    name: 'isEnabled',
    width: 1,
    hAlign: 'center',
    cell: (cell) => <CellCheckbox checked={cell.original.isEnabled} />,
  },
];
