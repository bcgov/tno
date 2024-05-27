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
    .reduce(
      (acc, item) => {
        let key = item.source?.name; // default to source
        if (!item?.source?.name) {
          key = item.otherSource;
        }
        switch (groupBy) {
          case 'time':
            const date = new Date(item.publishedOn);
            const hours = date.getHours().toString().padStart(2, '0');
            const minutes = date.getMinutes().toString().padStart(2, '0');
            key = `${moment(date).format('DD-MMM-YYYY')} ${`(${hours}:${minutes})`}`;
            break;
          case 'source':
            if (!item?.source?.name) {
              key = item.otherSource;
            } else {
              const shortName =
                item.source.shortName && item.source.shortName !== ''
                  ? ` (${item.source.shortName})`
                  : '';
              key = `${item.source.name}${shortName}`;
            }
            break;
        }
        if (!acc[key]) {
          acc[key] = [];
        }
        acc[key].push(item);
        return acc;
      },
      {} as Record<string, IContentSearchResult[]>,
    );
  return grouped;
};
