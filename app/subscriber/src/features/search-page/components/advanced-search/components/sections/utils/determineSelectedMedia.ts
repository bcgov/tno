import { IFilterSettingsModel, ISourceModel } from 'tno-core';

import { IGroupOption } from '../../../interfaces/IGroupOption';
import { sortableMediaOptions } from './sortableMediaOptions';

export const determineSelectedMedia = (filter: IFilterSettingsModel, options: IGroupOption[]) => {
  const mediaOptions = sortableMediaOptions(options);
  const selectedMedia = mediaOptions.filter((option) => {
    return filter.sourceIds?.includes(option.value);
  });
  return selectedMedia;
};
