import { defaultFilterSettings } from 'features/constants';
import { IFilterSettingsModel } from 'tno-core';

export const createFilterSettings = (
  startDate: string,
  endDate: string,
  // defaults to 500, manual override provided
  defaultSize?: number,
): IFilterSettingsModel => {
  return {
    ...defaultFilterSettings,
    startDate,
    endDate,
    size: defaultSize ?? defaultFilterSettings.size,
  };
};
