import { ContentTypeName, IContentModel } from 'tno-core';

import { IContentSearchResult } from './interfaces';

export const castToSearchResult = (content: IContentModel): IContentSearchResult => {
  return {
    ...content,
    hasTranscript: content.contentType === ContentTypeName.AudioVideo && !!content.body,
  };
};
