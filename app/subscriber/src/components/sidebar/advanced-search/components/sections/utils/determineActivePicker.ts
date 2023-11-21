import moment from 'moment';

import { QuickPickerNames } from '../constants';

/**
 * Function that helps determine which picker should be active when loading the advanced search page
 * @param publishedStartOn ISO string value of the published start date
 * @param publishedEndOn ISO string value of the published end date
 */
export const determineActivePicker = (publishedStartOn?: string, publishedEndOn?: string) => {
  // need to ensure end on date is today which means they are can be using the quick picker
  if (moment(publishedEndOn).endOf('day').isSame(moment().endOf('day'))) {
    if (moment(publishedStartOn).endOf('day').isSame(moment(publishedEndOn).endOf('day'))) {
      return QuickPickerNames.Today;
    } else if (
      moment(publishedStartOn).endOf('day').isSame(moment().subtract(24, 'hours').endOf('day'))
    ) {
      return QuickPickerNames.TwentyFourHours;
    } else if (
      moment(publishedStartOn).endOf('day').isSame(moment().subtract(7, 'day').endOf('day'))
    ) {
      return QuickPickerNames.SevenDays;
    } else if (
      moment(publishedStartOn).endOf('day').isSame(moment().subtract(48, 'hours').endOf('day'))
    ) {
      return QuickPickerNames.FortyEightHours;
    }
  } else {
    // custom picker
    return QuickPickerNames.Custom;
  }
};
