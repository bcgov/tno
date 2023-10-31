import { IFilterSettingsModel } from '../../hooks';
import { generateRangeForDateOffset } from './generateRangeForDateOffset';
import { generateRangeForDates } from './generateRangeForDates';

export const generatePublishedOnQuery = (settings: IFilterSettingsModel) => {
  if (settings.dateOffset !== undefined)
    return generateRangeForDateOffset(
      'publishedOn',
      settings.dateOffset === 3 && !settings.validDateOffset ? undefined : settings.dateOffset,
    );
  if (settings.startDate && settings.endDate)
    return generateRangeForDates('publishedOn', settings.startDate, settings.endDate);
  if (settings.startDate) return generateRangeForDates('publishedOn', settings.startDate);
  if (settings.endDate) return generateRangeForDates('publishedOn', null, settings.endDate);
};
