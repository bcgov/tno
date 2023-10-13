import { IFilterSettingsModel } from 'tno-core';

import { defaultFilterSettings } from '../constants';

export const createFilterSettings = (startDate: string, endDate: string): IFilterSettingsModel => {
  return { ...defaultFilterSettings, startDate, endDate };
};
