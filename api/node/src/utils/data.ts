import { BubbleDataPoint, ChartData, ChartTypeRegistry, Point } from 'chart.js';

/**
 * Convert JSON string value from base64 to ChartJS configuration object.
 * @param data Base64 string value.
 * @returns Object parsed from 'data'.
 */
export const convertBase64ConfigToChartJsConfig = (data: string) => {
  const chartJsConfigAsJson = Buffer.from(data, 'base64').toString('ascii');
  const chartJsConfig = JSON.parse(chartJsConfigAsJson);
  if (chartJsConfig.plugins?.datalabels?.formatter) {
    // the expectation here is for a lambda type function
    chartJsConfig.plugins.datalabels.formatter = eval(chartJsConfig.plugins.datalabels.formatter);
  }
  return chartJsConfig;
};

/**
 * Evaluate any possible scripts within the data or config.
 * @param data JSON object may contain chart config, or chart data.
 */
export const evalScripts = (
  data: ChartData<
    keyof ChartTypeRegistry,
    (number | [number, number] | Point | BubbleDataPoint | null)[],
    unknown
  >,
): ChartData<
  keyof ChartTypeRegistry,
  (number | [number, number] | Point | BubbleDataPoint | null)[],
  unknown
> => {
  if (data.datasets && data.datasets.length) {
    for (let i = 0; i < data.datasets.length; i++) {
      const dataset = data.datasets[i];
      try {
        const isArray = Array.isArray(dataset.backgroundColor);
        if (
          dataset.backgroundColor &&
          !isArray &&
          typeof dataset.backgroundColor === 'string' &&
          dataset.backgroundColor[0] === '('
        )
          dataset.backgroundColor = eval(dataset.backgroundColor);
      } catch (ex) {
        console.error('Failed to eval dataset.backgroundColor', ex);
        dataset.backgroundColor = undefined;
      }
      try {
        const isArray = Array.isArray(dataset.borderColor);
        if (
          dataset.borderColor &&
          !isArray &&
          typeof dataset.borderColor === 'string' &&
          dataset.borderColor[0] === '('
        )
          dataset.borderColor = eval(dataset.borderColor);
      } catch (ex) {
        console.error('Failed to eval dataset.borderColor', ex);
        dataset.borderColor = undefined;
      }
    }
  }

  return data;
};

/**
 * Convert JSON object to a base64 string.
 * @param data JSON object that will be converted to base64 string.
 * @returns Base64 string data.
 */
export const convertChartJsConfigToBase64String = (data: object) => {
  const json = JSON.stringify(data);
  const b = Buffer.from(json);
  return b.toString('base64');
};
