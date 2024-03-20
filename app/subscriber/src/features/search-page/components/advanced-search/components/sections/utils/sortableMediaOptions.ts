import { ISourceModel } from 'tno-core';

import { IGroupOption } from '../../../interfaces/IGroupOption';

export const sortableMediaOptions = (mediaSource: IGroupOption[]) => {
  return mediaSource
    .map((option) => ({
      label: option.name,
      value: option.id,
      sortOrder: option.sortOrder,
    }))
    .sort((a, b) => a.sortOrder - b.sortOrder);
};
