import { IReportSectionChartTemplateModel } from 'tno-core';

/**
 * Determine whether the chart's data comes from Elasticsearch aggregations instead of a list of stories.
 * @param chart The chart in the section.
 * @returns True if the API can provide the chart data.
 */
export const isAggregateChart = (chart: IReportSectionChartTemplateModel) => {
  // tno-core does not publish these settings yet.
  const settings = chart.settings as { aggregate?: boolean; aggregations?: any };
  return !!settings?.aggregate || !!settings?.aggregations;
};
