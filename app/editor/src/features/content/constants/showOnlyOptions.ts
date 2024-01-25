import { ActionName, OptionItem } from 'tno-core';

import { ShowOnlyValues } from './ShowOnlyValues';

export const showOnlyOptions = [
  new OptionItem(ShowOnlyValues.PrintContent, ShowOnlyValues.PrintContent),
  new OptionItem(ShowOnlyValues.HasTopic, ShowOnlyValues.HasTopic),
  new OptionItem(ActionName.Commentary, ActionName.Commentary),
  new OptionItem(ActionName.TopStory, ActionName.TopStory),
  new OptionItem(ShowOnlyValues.Published, ShowOnlyValues.Published),
  new OptionItem(ShowOnlyValues.PendingTranscript, ShowOnlyValues.PendingTranscript),
];
