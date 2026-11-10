import { ContentTypeName, IContentModel } from '@/shared';

import { IContentSearchResult } from './interfaces';

export const castToSearchResult = (content: IContentModel): IContentSearchResult => {
  return {
    ...content,
    hasTranscript: content.contentType === ContentTypeName.AudioVideo && !!content.body,
  };
};
