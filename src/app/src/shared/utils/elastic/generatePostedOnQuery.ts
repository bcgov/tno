import { IFilterSettingsModel } from '../../hooks';
import { generateRangeForDates } from './generateRangeForDates';

export const generatePostedOnQuery = (
  settings: Omit<IFilterSettingsModel, 'size' | 'searchUnpublished'>,
) => {
  if (settings.startPostedDate && settings.endPostedDate)
    return generateRangeForDates('postedOn', settings.startPostedDate, settings.endPostedDate);
  if (settings.startPostedDate) return generateRangeForDates('postedOn', settings.startPostedDate);
  if (settings.endPostedDate)
    return generateRangeForDates('postedOn', null, settings.endPostedDate);
};
