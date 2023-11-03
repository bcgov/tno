import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { IActionModel, IFilterActionSettingsModel } from 'tno-core';

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
  if (filter.topStory) {
    const action = actions.find((x) => x.name === 'Top Story');
    if (action)
      result.push({
        id: action.id,
        value: 'true',
      });
  }

  return result;
};
