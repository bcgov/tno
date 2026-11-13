import { IContentTonePoolModel, ITonePoolModel } from '@/shared';

import { defaultTonePool } from './defaultTonePool';

export const defaultContentTonePool: IContentTonePoolModel = {
  ...(defaultTonePool as ITonePoolModel),
  value: -99,
};
