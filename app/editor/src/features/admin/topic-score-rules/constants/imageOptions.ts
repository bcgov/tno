import { OptionItem } from 'tno-core';

/** The rule image condition: any, requires an image, or requires none. */
export const imageOptions = [
  new OptionItem('Any', ''),
  new OptionItem('Yes', 'true'),
  new OptionItem('No', 'false'),
];
