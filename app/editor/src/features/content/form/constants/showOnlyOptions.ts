import { ContentTypeName } from 'hooks';
import { OptionItem } from 'tno-core';

export const showOnlyOptions = [
  new OptionItem('Print Content', ContentTypeName.PrintContent),
  new OptionItem('Included in EoD', 'true'),
  new OptionItem('On Ticker', 'On Ticker'),
  new OptionItem('Commentary', 'Commentary'),
  new OptionItem('Top Story', 'Top Story'),
];
