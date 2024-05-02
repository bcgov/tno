import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';

import { IGroupByState } from '../interfaces';

const sortFunc = (key: string) => {
  switch (key) {
    case 'published':
      return (a: IContentSearchResult, b: IContentSearchResult) =>
        a.publishedOn < b.publishedOn ? 1 : -1;
    case 'source':
      return (a: IContentSearchResult, b: IContentSearchResult) => {
        if (a.source && b.source) {
          return a.source.sortOrder > b.source.sortOrder ? 1 : -1;
        }
        return -1;
      };
    default:
      return (a: IContentSearchResult, b: IContentSearchResult) =>
        a.publishedOn < b.publishedOn ? 1 : -1;
  }
};

export const groupContent = (groupBy: IGroupByState, content: IContentSearchResult[]) => {
  let firstSort = 'published';
  let secondSort = 'source';

  switch (groupBy) {
    case 'time':
      firstSort = 'source';
      secondSort = 'published';
      break;
    case 'source':
      firstSort = 'published';
      secondSort = 'source';
      break;
  }

  const grouped = content
    .sort(sortFunc(firstSort))
    .sort(sortFunc(secondSort))
    .reduce((acc, item) => {
      if (!item?.source?.name) {
        item.source = {
          id: 0,
          name: item.otherSource,
          description: item.otherSource,
          sortOrder: 0,
          isEnabled: true,
          code: item.otherSource,
          shortName: item.otherSource,
          licenseId: 0,
          mediaTypeSearchMappings: [],
          autoTranscribe: false,
          disableTranscribe: false,
          useInTopics: false,
          configuration: {},
          actions: [],
          metrics: [],
        };
      } // fill with other source name if source does not exist for grouping purposes
      let key = item.source?.name; // default to source
      switch (groupBy) {
        case 'time':
          const date = new Date(item.publishedOn);
          const hours = date.getHours().toString().padStart(2, '0');
          const minutes = date.getMinutes().toString().padStart(2, '0');
          key = `${moment(date).format('DD/MM/YY')} (${hours}:${minutes})`;
          break;
        case 'source':
          key = item.source.name;
          break;
      }
      if (!acc[key]) {
        acc[key] = [];
      }
      acc[key].push(item);
      return acc;
    }, {} as Record<string, IContentSearchResult[]>);
  return grouped;
};
