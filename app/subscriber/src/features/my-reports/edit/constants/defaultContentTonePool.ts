import { IContentTonePoolModel, ITonePoolModel } from 'tno-core';

import { defaultTonePool } from './defaultTonePool';

export const defaultContentTonePool: IContentTonePoolModel = {
  ...(defaultTonePool as ITonePoolModel),
  value: -99,
};
