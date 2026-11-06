/** A report that uses a chart template. */
export interface IChartTemplateUsageModel {
  id: number;
  name: string;
  sections: string[];
  owner?: string;
  isEnabled: boolean;
}
