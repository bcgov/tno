import { IContentState } from '../interfaces';
import { defaultContentFilter } from './defaultContentFilter';

export const initialContentState: IContentState = {
  frontPage: {
    content: undefined,
    filter: defaultContentFilter,
  },
  home: {
    content: undefined,
    filter: defaultContentFilter,
  },
  mediaType: {
    content: undefined,
    filter: defaultContentFilter,
  },
  myMinister: {
    content: undefined,
    filter: defaultContentFilter,
  },
  pressGalleryFilter: {
    dateFilter: null,
    pressFilter: null,
  },
  search: {
    content: undefined,
    filter: {
      ...defaultContentFilter,
      inByline: true,
      inHeadline: true,
      inStory: true,
      dateOffset: 1,
    },
  },
  todaysCommentary: {
    content: undefined,
    filter: defaultContentFilter,
  },
  topStories: {
    content: undefined,
    filter: defaultContentFilter,
  },
  avOverview: {
    filter: defaultContentFilter,
  },
};
