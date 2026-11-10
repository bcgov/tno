import { IChartSectionSettingsModel } from 'tno-core';

import { determineColor } from './determineColor';

export const determineBackgroundColor = (
  index: number,
  datasetLabel: string,
  chartSectionSettings: IChartSectionSettingsModel,
) => {
  return determineColor(
    chartSectionSettings.datasetColors,
    index,
    datasetLabel,
    chartSectionSettings,
  );
};
