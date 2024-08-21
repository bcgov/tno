import { IChartSectionSettingsModel } from 'tno-core';

import { determineColor } from './determineColor';

export const determineBorderColor = (
  index: number,
  datasetLabel: string,
  chartSectionSettings: IChartSectionSettingsModel,
) => {
  return determineColor(
    chartSectionSettings.datasetBorderColors,
    index,
    datasetLabel,
    chartSectionSettings,
  );
};
