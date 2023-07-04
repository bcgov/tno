import { IChartTemplateModel, ISortableModel } from '.';

export interface IReportTemplateModel extends ISortableModel<number> {
  subject: string;
  body: string;
  enableSections: boolean;
  enableSectionSummary: boolean;
  enableSummary: boolean;
  enableCharts: boolean;
  enableChartsOverTime: boolean;
  chartTemplates: IChartTemplateModel[];
}
