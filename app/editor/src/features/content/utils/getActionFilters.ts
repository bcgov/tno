import { IActionModel, IFilterActionSettingsModel } from 'tno-core';

import { ShowOnlyValues } from '../constants';
import { IContentListAdvancedFilter, IContentListFilter } from '../interfaces';

/**
 * Generate an array of action filters based on the specified 'filter' and array of 'actions'.
 * @param filter The filter.
 * @param actions An array of actions.
 * @returns An array of action filters.
 */
export const getActionFilters = (
  filter: IContentListFilter & Partial<IContentListAdvancedFilter>,
  actions: IActionModel[],
) => {
  const result: IFilterActionSettingsModel[] = [];
  if (filter.commentary) {
    const action = actions.find((x) => x.name === ShowOnlyValues.Commentary);
    if (action)
      result.push({
        id: action.id,
        value: '*',
        valueType: action.valueType,
      });
  }
  if (filter.topStory) {
    const action = actions.find((x) => x.name === ShowOnlyValues.TopStory);
    if (action)
      result.push({
        id: action.id,
        value: 'true',
      });
  }
  if (filter.homepage) {
    const action = actions.find((x) => x.name === ShowOnlyValues.Homepage);
    if (action)
      result.push({
        id: action.id,
        value: 'true',
      });
  }
  if (filter.homepage) {
    const action = actions.find((x) => x.name === ShowOnlyValues.OnTicker);
    if (action)
      result.push({
        id: action.id,
        value: 'true',
      });
  }
  return result;
};
