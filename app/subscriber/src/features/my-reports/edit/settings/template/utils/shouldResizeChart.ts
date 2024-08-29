import { IChartSectionSettingsModel } from 'tno-core';

/**
 * Determines if the size of the chart will fit the axis labels.
 * @param axisLabelCount The number of axis labels.
 * @param chartSectionSettings The chart section settings.
 * @param minAxisColumnWidth The minimum size of an axis column (defaults 30).
 * @returns True if the chart should resize.
 */
export const shouldResizeChart = (
  axisLabelCount: number,
  chartSectionSettings: IChartSectionSettingsModel,
  minAxisColumnWidth: number = 30,
) => {
  const size = !chartSectionSettings.isHorizontal
    ? chartSectionSettings.height
    : chartSectionSettings.width;
  if (!size || !axisLabelCount) return false;
  const axisColumnWidth = size / axisLabelCount;
  return axisColumnWidth < minAxisColumnWidth;
};
