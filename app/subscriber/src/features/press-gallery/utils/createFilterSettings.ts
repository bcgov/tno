import { IFilterSettingsModel } from 'tno-core';

import { defaultFilterSettings } from '../constants';

export const createFilterSettings = (
  startDate: string,
  endDate: string,
  defaultSize?: number,
): IFilterSettingsModel => {
  return {
    ...defaultFilterSettings,
    startDate,
    endDate,
    size: defaultSize ?? defaultFilterSettings.size,
  };
};
