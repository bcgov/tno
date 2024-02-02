import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';

import { IGroupByState } from '../interfaces';

export const groupContent = (groupBy: IGroupByState, content: IContentSearchResult[]) => {
  const grouped = content.reduce((acc, item) => {
    if (!item?.source?.name) return acc; // skip if no source
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
