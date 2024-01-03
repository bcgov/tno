import { ISourceModel } from 'tno-core';

export const sortableMediaOptions = (mediaSource: ISourceModel[]) => {
  return mediaSource
    .map((option) => ({
      label: option.name,
      value: option.id,
      sortOrder: option.sortOrder,
    }))
    .sort((a, b) => a.sortOrder - b.sortOrder);
};
