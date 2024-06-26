import React from 'react';
import { toast } from 'react-toastify';
import { useSettingsStore } from 'store/slices';
import { Settings } from 'tno-core';

import { useLookup } from './useLookup';

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
      const editorUrl = settings.find((s) => s.name === Settings.EditorUrl)?.value;
      const subscriberUrl = settings.find((s) => s.name === Settings.SubscriberUrl)?.value;
      const excludeBylineIds = settings.find((s) => s.name === Settings.ExcludeBylineIds)?.value;
      const excludeSourceIds = settings.find((s) => s.name === Settings.ExcludeSourceIds)?.value;
      const disableTranscriptionMediaTypeIds = settings.find(
        (s) => s.name === Settings.DisableTranscriptionMediaTypeIds,
      )?.value;
      const defaultReportTemplateId = settings.find(
        (s) => s.name === Settings.DefaultReportTemplate,
      )?.value;
      const frontpageFilterId = settings.find((s) => s.name === Settings.FrontpageFilter)?.value;
      const frontPageImageMediaTypeId = settings.find(
        (s) => s.name === Settings.FrontPageImageMediaType,
      )?.value;
      const eventOfTheDayReportId = settings.find(
        (s) => s.name === Settings.EventOfTheDayReport,
      )?.value;
      storeValues({
        loadingState: 1,
        isReady,
        commentaryActionId: commentaryActionId ? +commentaryActionId : undefined,
        topStoryActionId: topStoryActionId ? +topStoryActionId : undefined,
        featuredStoryActionId: featuredStoryActionId ? +featuredStoryActionId : undefined,
        alertActionId: alertActionId ? +alertActionId : undefined,
        editorUrl: editorUrl ? editorUrl : undefined,
        subscriberUrl: subscriberUrl ? subscriberUrl : undefined,
        defaultReportTemplateId: defaultReportTemplateId ? +defaultReportTemplateId : undefined,
        excludeBylineIds: excludeBylineIds ? excludeBylineIds.split(',').map((id) => +id) : [],
        excludeSourceIds: excludeSourceIds ? excludeSourceIds.split(',').map((id) => +id) : [],
        disableTranscriptionMediaTypeIds: disableTranscriptionMediaTypeIds
          ? disableTranscriptionMediaTypeIds.split(',').map((id) => +id)
          : [],
        frontpageFilterId: frontpageFilterId ? +frontpageFilterId : undefined,
        frontPageImageMediaTypeId: frontPageImageMediaTypeId
          ? +frontPageImageMediaTypeId
          : undefined,
        eventOfTheDayReportId: eventOfTheDayReportId ? +eventOfTheDayReportId : undefined,
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

      if (!values.editorUrl)
        toast.error(`Configuration "${Settings.SubscriberUrl}" is missing from settings.`);

      if (!values.subscriberUrl)
        toast.error(`Configuration "${Settings.EditorUrl}" is missing from settings.`);

      if (!values.defaultReportTemplateId)
        toast.error(`Configuration "${Settings.DefaultReportTemplate}" is missing from settings.`);

      if (!values.excludeBylineIds)
        toast.error(`Configuration "${Settings.ExcludeBylineIds}" is missing from settings.`);

      if (!values.excludeSourceIds)
        toast.error(`Configuration "${Settings.ExcludeSourceIds}" is missing from settings.`);

      if (!values.frontpageFilterId)
        toast.error(`Configuration "${Settings.FrontpageFilter}" is missing from settings.`);

      if (!values.frontPageImageMediaTypeId)
        toast.error(
          `Configuration "${Settings.FrontPageImageMediaType}" is missing from settings.`,
        );

      if (!values.disableTranscriptionMediaTypeIds)
        toast.error(
          `Configuration "${Settings.DisableTranscriptionMediaTypeIds}" is missing from settings.`,
        );

      if (!values.eventOfTheDayReportId)
        toast.error(`Configuration "${Settings.EventOfTheDayReport}" is missing from settings.`);
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
    values.disableTranscriptionMediaTypeIds,
    values.editorUrl,
    values.subscriberUrl,
    values.defaultReportTemplateId,
    values.frontpageFilterId,
    values.excludeBylineIds,
    values.excludeSourceIds,
    values.eventOfTheDayReportId,
    values.frontPageImageMediaTypeId,
  ]);

  return values;
};
