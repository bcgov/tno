import { IFilterSettingsModel } from 'tno-core';

export const defaultFilterSettings: IFilterSettingsModel = {
  searchUnpublished: false,
  size: 0,
  inByline: true,
  inHeadline: true,
  inStory: true,
  sourceIds: [],
  productIds: [],
  seriesIds: [],
  contributorIds: [],
  actions: [],
  contentTypes: [],
  tags: [],
  sentiment: [],
  defaultSearchOperator: 'and',
};
