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

  const getActionFilters = React.useCallback(() => {
    const result: IFilterActionSettingsModel[] = [];
    const commentaryAction = actions.find((x) => x.id === commentaryActionId);
    if (commentaryAction)
      result.push({
        id: commentaryAction.id,
        value: '*',
        valueType: commentaryAction.valueType,
      });
    const topStory = actions.find((x) => x.id === topStoryActionId);
    if (topStory)
      result.push({
        id: topStory.id,
        value: 'true',
      });
    const featuredStory = actions.find((x) => x.id === featuredStoryActionId);
    if (featuredStory)
      result.push({
        id: featuredStory.id,
        value: 'true',
      });
    return result;
  }, [actions, commentaryActionId, featuredStoryActionId, topStoryActionId]);

  return getActionFilters;
};
