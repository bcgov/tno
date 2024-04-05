import React from 'react';
import { toast } from 'react-toastify';
import { useSettingsStore } from 'store/slices';
import { Settings } from 'tno-core';

import { useLookup } from './useLookup';

export interface ISettings {
  isReady: boolean;
  commentaryActionId?: number;
  topStoryActionId?: number;
  featuredStoryActionId?: number;
  alertActionId?: number;
}

/**
 * Provides a helper to centralize configuration setting values.
 * @param validate Whether to validate the settings configurations.
 * @returns Configuration setting values.
 */
export const useSettings = (validate?: boolean) => {
  const [{ isReady, settings }] = useLookup();
  const [values, { storeValues, storeLoading }] = useSettingsStore();

  React.useEffect(() => {
    if (isReady && values.loadingState === 0) {
      const commentaryActionId = settings.find((s) => s.name === Settings.CommentaryAction)?.value;
      const topStoryActionId = settings.find((s) => s.name === Settings.TopStoryAction)?.value;
      const featuredStoryActionId = settings.find((s) => s.name === Settings.FeaturedAction)?.value;
      const alertActionId = settings.find((s) => s.name === Settings.AlertAction)?.value;
      storeValues({
        loadingState: 1,
        isReady,
        commentaryActionId: commentaryActionId ? +commentaryActionId : undefined,
        topStoryActionId: topStoryActionId ? +topStoryActionId : undefined,
        featuredStoryActionId: featuredStoryActionId ? +featuredStoryActionId : undefined,
        alertActionId: alertActionId ? +alertActionId : undefined,
      });
    }
  }, [values.loadingState, isReady, settings, storeValues]);

  React.useEffect(() => {
    if (values.loadingState === 1 && validate) {
      if (!values.commentaryActionId)
        toast.error(`Configuration "${Settings.CommentaryAction}" is missing from settings.`);

      if (!values.topStoryActionId)
        toast.error(`Configuration "${Settings.TopStoryAction}" is missing from settings.`);

      if (!values.featuredStoryActionId)
        toast.error(`Configuration "${Settings.FeaturedAction}" is missing from settings.`);

      if (!values.alertActionId)
        toast.error(`Configuration "${Settings.AlertAction}" is missing from settings.`);
      storeLoading(2);
    }
  }, [
    validate,
    storeLoading,
    values.alertActionId,
    values.commentaryActionId,
    values.featuredStoryActionId,
    values.topStoryActionId,
    values.loadingState,
  ]);

  return values;
};
