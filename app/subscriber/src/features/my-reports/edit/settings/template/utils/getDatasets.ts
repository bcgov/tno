import moment from 'moment';
import {
  getDistinct,
  IReportInstanceContentModel,
  IReportSectionChartTemplateModel,
  TopicTypeName,
} from 'tno-core';

import { IGenerateChartDataOptions } from './generateChartData';
import { getSentimentLabel } from './getSentimentLabel';
import { getSentimentValue } from './getSentimentValue';
import { isStoryCount } from './isStoryCount';
import { separateDatasets } from './separateDatasets';

export const getDatasets = (
  chart: IReportSectionChartTemplateModel,
  content: IReportInstanceContentModel[],
  options?: IGenerateChartDataOptions,
) => {
  const count = isStoryCount(chart);
  const key = chart.sectionSettings.dataset;
  let results: Record<string, IReportInstanceContentModel[]> = {};
  let data = content;

  // Remove stories without a sentiment value.
  if (chart.sectionSettings.excludeEmptyValues && !count)
    data = content.filter(
      (c) => c.content?.tonePools.length && c.content.tonePools[0].value !== undefined,
    );

  // Separate content into datasets.
  if (key === '') {
    // All content goes into a single dataset.
    results[count ? 'Stories' : 'Average Sentiment'] = data;
  } else if (key === 'mediaType') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => c?.content?.mediaTypeId,
      getLabel: (c) => c?.content?.mediaType?.name,
    });
  } else if (key === 'source') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => c?.content?.otherSource,
      getLabel: (c) => c?.content?.source?.name,
    });
  } else if (key === 'series') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => c?.content?.seriesId,
      getLabel: (c) => c?.content?.series?.name,
    });
  } else if (key === 'byline') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => c?.content?.byline,
    });
  } else if (key === 'contentType') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => c?.content?.contentType,
    });
  } else if (key === 'topicType') {
    const items = chart.sectionSettings.excludeEmptyValues
      ? data.filter((d) => (d.content?.tonePools.length ?? 0) > 0)
      : data;
    results = separateDatasets(items, {
      ...options,
      groups: [TopicTypeName.Proactive, TopicTypeName.Issues],
      groupOn: (c) => undefined,
      isInGroup: (c, g) => c?.content?.topics.some((t) => t.topicType === g) ?? false,
    });
  } else if (key === 'topicName') {
    const topics = getDistinct(data.map((d) => d.content?.topics ?? []).flat(), (c) => c.name).map(
      (t) => t.name,
    );
    results = separateDatasets(data, {
      ...options,
      groups: topics,
      groupOn: (c) => undefined,
      isInGroup: (c, g) => c?.content?.topics.some((t) => t.name === g) ?? false,
    });
  } else if (key === 'sentiment') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => getSentimentValue(c?.content),
    });
  } else if (key === 'sentimentSimple') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => getSentimentLabel(c?.content),
    });
  } else if (key === 'dayMonthYear') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => moment(c?.content?.publishedOn).tz('America/Vancouver').format('DD-MMM-YYYY'),
    });
  } else if (key === 'monthDay') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => moment(c?.content?.publishedOn).tz('America/Vancouver').format('MMM-DD'),
    });
  } else if (key === 'monthYear') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => moment(c?.content?.publishedOn).tz('America/Vancouver').format('MMM-YYYY'),
    });
  } else if (key === 'year') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => moment(c?.content?.publishedOn).tz('America/Vancouver').year(),
    });
  } else if (key === 'reportSection') {
    results = separateDatasets(data, {
      ...options,
      groupOn: (c) => c?.sectionName,
    });
  }

  return results;
};
