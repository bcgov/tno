import { IContentModel, IReportSectionChartTemplateModel } from 'tno-core';

import { calcAverageSentiment } from './calcAverageSentiment';
import { isStoryCount } from './isStoryCount';

export const calcDataPoint = (
  chart: IReportSectionChartTemplateModel,
  data: (IContentModel | undefined)[],
) => {
  return isStoryCount(chart) ? data.length : calcAverageSentiment(data);
};
