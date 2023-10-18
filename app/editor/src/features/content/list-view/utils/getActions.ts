import { IActionModel, IFilterActionSettingsModel } from 'tno-core';

import { IContentListFilter } from '../interfaces';

/**
 * Creates an array of actions from the provided filter information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param filter Filter object
 * @returns An array of actions.
 */
export const getActions = (filter: IContentListFilter, actions: IActionModel[]) => {
  const result: IFilterActionSettingsModel[] = [];
  if (filter.commentary) {
    const action = actions.find((x) => x.name === 'Commentary');
    if (action)
      result.push({
        id: action.id,
        value: action.valueLabel,
        valueType: action.valueType,
      });
  }
  if (filter.topStory) {
    const action = actions.find((x) => x.name === 'Top Story');
    if (action)
      result.push({
        id: action.id,
        value: action.valueLabel,
        valueType: action.valueType,
      });
  }
  return result;
};
