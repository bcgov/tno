import { IChartSectionSettingsModel } from '@/shared';

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

  // An empty list is the same as no list: Chart.js would draw a colour it cannot find in black,
  // so the chart keeps the colours Chart.js gives it.
  if (chartSectionSettings.applyColorToValue) return colors.length ? colors : undefined;

  if (dataset === '' && datasetValueProp === 'sentiment') {
    return selectSentimentColorForValue(0, colors);
  }
  if (dataset === 'sentiment' || dataset === 'sentimentSimple') {
    return selectSentimentColorForDataset(datasetLabel, colors);
  }
  return colors.length ? selectColorForDataset(index, colors) : undefined;
};
