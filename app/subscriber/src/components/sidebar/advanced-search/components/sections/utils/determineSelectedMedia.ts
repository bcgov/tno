import { IContentListFilter } from 'features/content/list-view/interfaces';
import { ISourceModel } from 'tno-core';

import { sortableMediaOptions } from './sortableMediaOptions';

export const determineSelectedMedia = (filter: IContentListFilter, options: ISourceModel[]) => {
  const mediaOptions = sortableMediaOptions(options);
  const selectedMedia = mediaOptions.filter((option) => {
    return filter.sourceIds?.includes(option.value);
  });
  return selectedMedia;
};
