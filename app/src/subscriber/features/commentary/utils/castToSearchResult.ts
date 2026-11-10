import { IContentMessageModel } from 'tno-core';

import { IContentSearchResult } from '@/subscriber/features/utils/interfaces';

export const castToSearchResult = (message: IContentMessageModel): IContentSearchResult => {
  return {
    ...message,
    hasTranscript: false,
    summary: '',
    body: '',
    isHidden: false,
    isPrivate: false,
    tags: [],
    labels: [],
    topics: [],
    timeTrackings: [],
    links: [],
    quotes: [],
    userNotifications: [],
    versions: {},
  };
};
