export interface IChartResultModel {
  /** Chart.JS JSON containing data */
  json: unknown;
  /** Elasticsearch query result */
  result?: unknown;
}
