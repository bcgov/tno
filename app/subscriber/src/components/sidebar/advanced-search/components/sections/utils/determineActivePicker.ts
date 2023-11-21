import { QuickPickerNames } from '../constants';

/**
 * Function that helps determine which picker should be active when loading the advanced search page
 * @param publishedStartOn ISO string value of the published start date
 * @param publishedEndOn ISO string value of the published end date
 */
export const determineActivePicker = (dateOffset: number) => {
  switch (dateOffset) {
    case 0:
      return QuickPickerNames.Today;
    case 1:
      return QuickPickerNames.TwentyFourHours;
    case 2:
      return QuickPickerNames.FortyEightHours;
    case 3:
      return QuickPickerNames.SevenDays;
    default:
      return QuickPickerNames.Custom;
  }
};
