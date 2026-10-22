import { IFilterSettingsModel } from '../../../hooks';

/** Settings that describe how results are shown rather than which stories match. */
const presentationKeys = ['size', 'from', 'sort', 'searchUnpublished', 'extendedQuery'];

/**
 * The settings that select stories, as [name, value] pairs, leaving out empty values.
 * @param settings Filter settings.
 * @returns The settings to show.
 */
export const getFilterSettingsSummary = (settings?: IFilterSettingsModel): [string, string][] =>
  Object.entries(settings ?? {})
    .filter(([key, value]) => {
      if (presentationKeys.includes(key)) return false;
      if (value === undefined || value === null || value === '' || value === false) return false;
      if (Array.isArray(value) && value.length === 0) return false;
      return true;
    })
    .map(([key, value]) => [key, typeof value === 'string' ? value : JSON.stringify(value)]);
