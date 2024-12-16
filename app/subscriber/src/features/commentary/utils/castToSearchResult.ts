import { IContentSearchResult } from 'features/utils/interfaces';
import { IContentMessageModel } from 'tno-core';

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
