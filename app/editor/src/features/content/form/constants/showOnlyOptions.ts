import { OptionItem } from 'tno-core';

import { ShowOnlyValues } from './ShowOnlyValues';

export const showOnlyOptions = [
  new OptionItem('Print Content', ShowOnlyValues.PrintContent),
  new OptionItem('Included in EoD', ShowOnlyValues.IncludedInEod),
  new OptionItem('Commentary', ShowOnlyValues.Commentary),
  new OptionItem('Top Story', ShowOnlyValues.TopStory),
];
