import { IFilterSettingsModel } from 'tno-core';

export const defaultFilterSettings: IFilterSettingsModel = {
  from: 0,
  size: 500,
  searchUnpublished: false,
  inByline: true,
  inHeadline: true,
  inStory: true,
  inProgram: true,
  sourceIds: [],
  mediaTypeIds: [],
  seriesIds: [],
  contributorIds: [],
  actions: [],
  contentTypes: [],
  tags: [],
  sentiment: [],
  defaultOperator: 'and',
};
