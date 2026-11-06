export interface IReportInstanceChartModel {
  instanceId: number;
  sectionName: string;
  sectionId: number;
  chartTemplateId: number;
  /** The Chart.js data. */
  data: any;
  /** The number of stories counted. */
  docCount: number;
  /** Whether a bucket list was longer than the aggregation returns. */
  isTruncated: boolean;
  generatedOn: string;
}
