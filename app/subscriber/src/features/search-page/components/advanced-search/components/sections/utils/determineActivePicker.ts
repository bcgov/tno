import { IFilterSettingsModel } from 'tno-core';

import { QuickPickerNames } from '../constants';

/**
 * Function that helps determine which picker should be active when loading the advanced search page
 * @param publishedStartOn ISO string value of the published start date
 * @param publishedEndOn ISO string value of the published end date
 */
export const determineActivePicker = (filter: IFilterSettingsModel) => {
  if (filter.startDate || filter.endDate) return QuickPickerNames.Custom;
  switch (filter.dateOffset) {
    case 0:
      return QuickPickerNames.Today;
    case 1:
      return QuickPickerNames.TwentyFourHours;
    case 2:
      return QuickPickerNames.FortyEightHours;
    case 7:
      return QuickPickerNames.SevenDays;
    case 30:
      return QuickPickerNames.OneMonth;
    case 60:
      return QuickPickerNames.TwoMonths;
    case 90:
      return QuickPickerNames.ThreeMonths;
    case 120:
      return QuickPickerNames.SixMonths;
    default:
      return QuickPickerNames.All;
  }
};
