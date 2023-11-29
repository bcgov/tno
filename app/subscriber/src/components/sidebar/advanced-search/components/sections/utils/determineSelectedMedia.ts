import { IFilterSettingsModel, ISourceModel } from 'tno-core';

import { sortableMediaOptions } from './sortableMediaOptions';

export const determineSelectedMedia = (filter: IFilterSettingsModel, options: ISourceModel[]) => {
  const mediaOptions = sortableMediaOptions(options);
  const selectedMedia = mediaOptions.filter((option) => {
    return filter.sourceIds?.includes(option.value);
  });
  return selectedMedia;
};
