import { IAVOverviewSectionModel } from 'tno-core';

export const defaultAVOverviewSection = (instanceId: number): IAVOverviewSectionModel => ({
  id: 0,
  name: '',
  instanceId,
  otherSource: '',
  anchors: '',
  startTime: '',
  items: [],
  sortOrder: 0,
});
