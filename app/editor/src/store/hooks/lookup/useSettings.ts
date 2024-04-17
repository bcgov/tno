import React from 'react';
import { toast } from 'react-toastify';
import { Settings } from 'tno-core';

import { useLookup } from './useLookup';

export interface ISettings {
  isReady: boolean;
  commentaryActionId?: number;
  topStoryActionId?: number;
  featuredStoryActionId?: number;
  alertActionId?: number;
  frontPageImagesMediaTypeId?: number;
}

/**
 * Provides a helper to centralize configuration setting values.
 * @param validate Whether to validate the settings configurations.
 * @returns Configuration setting values.
 */
export const useSettings = (validate?: boolean) => {
  const [{ isReady, settings }] = useLookup();

  const [values, setValues] = React.useState<ISettings>({ isReady: false }); // TODO: Move to redux store for single data management.
  const [isLoaded, setIsLoaded] = React.useState(0);

  React.useEffect(() => {
    if (isReady) {
      const commentaryActionId = settings.find((s) => s.name === Settings.CommentaryAction)?.value;
      const topStoryActionId = settings.find((s) => s.name === Settings.TopStoryAction)?.value;
      const featuredStoryActionId = settings.find((s) => s.name === Settings.FeaturedAction)?.value;
      const alertActionId = settings.find((s) => s.name === Settings.AlertAction)?.value;
      const frontPageImagesMediaTypeId = settings.find(
        (s) => s.name === Settings.FrontPageImageMediaType,
      )?.value;
      setValues({
        isReady,
        commentaryActionId: commentaryActionId ? +commentaryActionId : undefined,
        topStoryActionId: topStoryActionId ? +topStoryActionId : undefined,
        featuredStoryActionId: featuredStoryActionId ? +featuredStoryActionId : undefined,
        alertActionId: alertActionId ? +alertActionId : undefined,
        frontPageImagesMediaTypeId: frontPageImagesMediaTypeId
          ? +frontPageImagesMediaTypeId
          : undefined,
      });
      setIsLoaded(1);
    }
  }, [isReady, settings]);

  React.useEffect(() => {
    if (isLoaded === 1 && validate) {
      if (!values.commentaryActionId)
        toast.error(`Configuration "${Settings.CommentaryAction}" is missing from settings.`);

      if (!values.topStoryActionId)
        toast.error(`Configuration "${Settings.TopStoryAction}" is missing from settings.`);

      if (!values.featuredStoryActionId)
        toast.error(`Configuration "${Settings.FeaturedAction}" is missing from settings.`);

      if (!values.alertActionId)
        toast.error(`Configuration "${Settings.AlertAction}" is missing from settings.`);

      if (!values.frontPageImagesMediaTypeId)
        toast.error(
          `Configuration "${Settings.FrontPageImageMediaType}" is missing from settings.`,
        );
      setIsLoaded(2);
    }
  }, [
    validate,
    isLoaded,
    values.alertActionId,
    values.commentaryActionId,
    values.featuredStoryActionId,
    values.topStoryActionId,
    values.frontPageImagesMediaTypeId,
  ]);

  return values;
};
