import { OptionItem } from 'tno-core';

import { ShowOnlyValues } from './ShowOnlyValues';

export const showOnlyOptions = [
  new OptionItem(ShowOnlyValues.PrintContent, ShowOnlyValues.PrintContent),
  new OptionItem(ShowOnlyValues.HasTopic, ShowOnlyValues.HasTopic),
  new OptionItem(ShowOnlyValues.Commentary, ShowOnlyValues.Commentary),
  new OptionItem(ShowOnlyValues.TopStory, ShowOnlyValues.TopStory),
  new OptionItem(ShowOnlyValues.Published, ShowOnlyValues.Published),
  new OptionItem(ShowOnlyValues.PendingTranscript, ShowOnlyValues.PendingTranscript),
];
