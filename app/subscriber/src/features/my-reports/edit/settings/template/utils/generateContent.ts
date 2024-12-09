import moment from 'moment';
import { ContentStatusName, ContentTypeName, IContentModel } from 'tno-core';

import { generateContentTopic } from './generateContentTopic';
import { generateMediaType } from './generateMediaType';
import { generateRandom } from './generateRandom';
import { generateSeries } from './generateSeries';
import { generateSource } from './generateSource';

interface IGenerateContentOptions {
  _id?: number;
  content?: Partial<IContentModel>;
}

export const generateContent = (options: IGenerateContentOptions) => {
  const source = generateSource({ _id: generateRandom(1, 10) });
  const mediaType = generateMediaType({ _id: generateRandom(1, 5) });
  const series = generateSeries({ _id: generateRandom(1, 10) });
  const date = moment(new Date()).subtract(generateRandom(1, 50) - 1, 'days');
  const topic = generateContentTopic({ _id: generateRandom(1, 10) });
  const entity: IContentModel = {
    id: options?._id ?? 0,
    status: ContentStatusName.Draft,
    contentType: Object.values(ContentTypeName)[generateRandom(1, 4) - 1],
    licenseId: 0,
    source: source,
    otherSource: source.code,
    mediaTypeId: mediaType.id,
    mediaType: mediaType,
    seriesId: series.id,
    series: series,
    headline: `Story ${options?._id ?? 0}`,
    byline: `Byline ${generateRandom(1, 10)}`,
    edition: `Edition ${generateRandom(1, 5)}`,
    section: `Section ${generateRandom(1, 5)}`,
    page: `A${generateRandom(1, 5)}`,
    postedOn: date.toISOString(),
    publishedOn: date.toISOString(),
    summary: '',
    body: '',
    isHidden: false,
    isApproved: false,
    isPrivate: false,
    actions: [],
    tags: [],
    labels: [],
    topics: generateRandom(0, 1) ? [topic] : [],
    tonePools: [
      {
        id: 0,
        value: generateRandom(-5, 5),
        name: 'Default Pool',
        ownerId: 0,
        isPublic: true,
        description: '',
        sortOrder: 0,
        isEnabled: true,
      },
    ],
    timeTrackings: [],
    fileReferences: [],
    links: [],
    quotes: [],
    userNotifications: [],
    versions: {},
    isCBRAUnqualified: false,
    ...options.content,
  };
  return entity;
};

export const generateContents = (min: number, max: number) => {
  const quantity = Math.ceil(generateRandom(min, max));
  return Array.from(Array(quantity).keys()).map((key) => generateContent({ _id: key }));
};
