import { IChartSectionSettingsModel } from 'tno-core';

import { selectColorForDataset } from './selectColorForDataset';
import { selectSentimentColorForDataset } from './selectSentimentColorForDataset';
import { selectSentimentColorForValue } from './selectSentimentColorForValue';

export const determineColor = (
  colors: string[] | undefined,
  index: number,
  datasetLabel: string,
  chartSectionSettings: IChartSectionSettingsModel,
) => {
  const dataset = chartSectionSettings.dataset;
  const datasetValueProp = chartSectionSettings.datasetValue;

  if (!colors) return undefined;

  if (chartSectionSettings.applyColorToValue) return colors;

  if (dataset === '' && datasetValueProp === 'sentiment') {
    return selectSentimentColorForValue(0, colors);
  }
  if (dataset === 'sentiment' || dataset === 'sentimentSimple') {
    return selectSentimentColorForDataset(datasetLabel, colors);
  }
  return selectColorForDataset(index, colors);
};
