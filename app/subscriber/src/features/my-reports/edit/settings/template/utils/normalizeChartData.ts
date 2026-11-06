import { IChartSectionSettingsModel } from 'tno-core';

import { IChartData } from './IChartData';

/**
 * Prepare chart data from the API for Chart.js in the browser. The API provides the colours for
 * the Charts API, which can evaluate a function that picks the colour of each sentiment value.
 * @param data The chart data from the API.
 * @param settings The chart settings.
 * @returns The chart data to display.
 */
export const normalizeChartData = (
  data: IChartData,
  settings?: IChartSectionSettingsModel,
): IChartData => {
  const [positive = 'green', neutral = 'gold', negative = 'red'] = settings?.datasetColors ?? [];
  const toColors = (value: any, values: (number | null | undefined)[]) => {
    if (typeof value !== 'string') return value ?? undefined;
    return values.map((v) =>
      v == null ? neutral : v > 0 ? positive : v === 0 ? neutral : negative,
    );
  };
  return {
    ...data,
    datasets: data.datasets.map((dataset: any) => ({
      ...dataset,
      backgroundColor: toColors(dataset.backgroundColor, dataset.data),
      borderColor: toColors(dataset.borderColor, dataset.data),
    })),
  };
};
