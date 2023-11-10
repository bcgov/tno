import { IFilterSettingsModel } from 'tno-core';

export const defaultFilterSettings: IFilterSettingsModel = {
  from: 0,
  size: 500,
  searchUnpublished: false,
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
