/**
 * Convert JSON string value from base64 to ChartJS configuration object.
 * @param data Base64 string value.
 * @returns Object parsed from 'data'.
 */
export const convertBase64ConfigToChartJsConfig = (data: string) => {
  const chartJsConfigAsJson = Buffer.from(data, 'base64').toString('ascii');
  const chartJsConfig = JSON.parse(chartJsConfigAsJson);
  return chartJsConfig;
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
