import { IContentListAdvancedFilter, IContentListFilter } from 'features/content/interfaces';
import React from 'react';
import { useLookup, useSettings } from 'store/hooks';
import { IFilterActionSettingsModel } from 'tno-core';

/**
 * Generate an array of action filters based on the specified 'filter' and array of 'actions'.
 * @param filter The filter.
 * @param actions An array of actions.
 * @returns An array of action filters.
 */
export const useActionFilters = () => {
  const [{ actions }] = useLookup();
  const { commentaryActionId, topStoryActionId, featuredStoryActionId } = useSettings();

  const getActionFilters = React.useCallback(
    (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      const result: IFilterActionSettingsModel[] = [];
      if (filter.commentary) {
        const action = actions.find((x) => x.id === commentaryActionId);
        if (action)
          result.push({
            id: action.id,
            value: '*',
            valueType: action.valueType,
          });
      }
      if (filter.topStory) {
        const action = actions.find((x) => x.id === topStoryActionId);
        if (action)
          result.push({
            id: action.id,
            value: 'true',
          });
      }
      if (filter.featuredStory) {
        const action = actions.find((x) => x.id === featuredStoryActionId);
        if (action)
          result.push({
            id: action.id,
            value: 'true',
          });
      }
      return result;
    },
    [actions, commentaryActionId, featuredStoryActionId, topStoryActionId],
  );

  return getActionFilters;
};
