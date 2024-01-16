import { IActionModel, IFilterActionSettingsModel, IFilterSettingsModel } from 'tno-core';

/**
 * Generate an array of action filters based on the specified 'filter' and array of 'actions'.
 * @param filter The filter.
 * @param actions An array of actions.
 * @returns An array of action filters.
 */
export const getActionFilters = (filter: IFilterSettingsModel, actions: IActionModel[]) => {
  const result: IFilterActionSettingsModel[] = [];
  if (filter.topStory) {
    const action = actions.find((x) => x.name === 'Top Story');
    if (action)
      result.push({
        id: action.id,
        value: 'true',
      });
  }
  if (filter.featured) {
    const action = actions.find((x) => x.name === 'Homepage');
    if (action)
      result.push({
        id: action.id,
        value: 'true',
      });
  }

  if (filter.commentary) {
    const action = actions.find((x) => x.name === 'Commentary');
    if (action)
      result.push({
        id: action.id,
        value: '*',
        valueType: action.valueType,
      });
  }

  return result;
};
