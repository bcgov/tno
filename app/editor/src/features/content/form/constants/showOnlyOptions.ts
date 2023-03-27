import { OptionItem } from 'tno-core';

import { ShowOnlyValues } from '.';

export const showOnlyOptions = [
  new OptionItem(ShowOnlyValues.PrintContent, ShowOnlyValues.PrintContent),
  new OptionItem(ShowOnlyValues.HasTopic, ShowOnlyValues.HasTopic),
  new OptionItem(ShowOnlyValues.Commentary, ShowOnlyValues.Commentary),
  new OptionItem(ShowOnlyValues.TopStory, ShowOnlyValues.TopStory),
];
