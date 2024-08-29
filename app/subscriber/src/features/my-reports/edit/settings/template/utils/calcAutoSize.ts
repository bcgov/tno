import { IChartSectionSettingsModel } from 'tno-core';

/**
 * Determines if the size of the chart will fit the axis labels.
 * If not it will generate a new chart section settings.
 * @param axisLabelCount The number of axis labels.
 * @param chartSectionSettings The chart section settings.
 * @param minAxisColumnWidth The minimum size of an axis column (defaults 30).
 * @returns Updated chart section settings.
 */
export const calcAutoSize = (
  axisLabelCount: number,
  chartSectionSettings: IChartSectionSettingsModel,
  minAxisColumnWidth: number = 30,
) => {
  const size = chartSectionSettings.isHorizontal
    ? chartSectionSettings.width
    : chartSectionSettings.height;
  if (!size || !axisLabelCount) return chartSectionSettings;
  const axisColumnWidth = size / axisLabelCount;
  if (axisColumnWidth < minAxisColumnWidth) {
    const newSize = minAxisColumnWidth * axisLabelCount;
    const width = chartSectionSettings.isHorizontal ? newSize : chartSectionSettings.width;
    const height = chartSectionSettings.isHorizontal ? chartSectionSettings.height : newSize;
    return {
      ...chartSectionSettings,
      width,
      height,
      aspectRatio: height ? undefined : chartSectionSettings.aspectRatio,
    };
  }
  return chartSectionSettings;
};
