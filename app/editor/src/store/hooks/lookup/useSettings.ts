import React from 'react';
import { toast } from 'react-toastify';
import { ISettingsState, useSettingsStore } from 'store/slices';
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
export const useSettings = (validate?: boolean): ISettingsState => {
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
      const defaultReportTemplateId = settings.find(
        (s) => s.name === Settings.DefaultReportTemplate,
      )?.value;
      const frontpageImageMediaTypeId = settings.find(
        (s) => s.name === Settings.FrontPageImageMediaType,
      )?.value;
      const frontpageFilterId = settings.find((s) => s.name === Settings.FrontpageFilter)?.value;
      const morningReportId = settings.find((s) => s.name === Settings.MorningReport)?.value;
      const frontPageImagesReportId = settings.find(
        (s) => s.name === Settings.FrontPageImagesReport,
      )?.value;
      const topStoryAlertId = settings.find((s) => s.name === Settings.TopStoryAlert)?.value;

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
        frontpageFilterId: frontpageFilterId ? +frontpageFilterId : undefined,
        morningReportId: morningReportId ? +morningReportId : undefined,
        frontpageImageMediaTypeId: frontpageImageMediaTypeId
          ? +frontpageImageMediaTypeId
          : undefined,
        frontPageImagesReportId: frontPageImagesReportId ? +frontPageImagesReportId : undefined,
        topStoryAlertId: topStoryAlertId ? +topStoryAlertId : undefined,
        excludeBylineIds: excludeBylineIds ? excludeBylineIds.split(',').map((id) => +id) : [],
        excludeSourceIds: excludeSourceIds ? excludeSourceIds.split(',').map((id) => +id) : [],
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

      if (!values.frontpageImageMediaTypeId)
        toast.error(
          `Configuration "${Settings.FrontPageImageMediaType}" is missing from settings.`,
        );

      if (!values.frontpageFilterId)
        toast.error(`Configuration "${Settings.FrontpageFilter}" is missing from settings.`);

      if (!values.morningReportId)
        toast.error(`Configuration "${Settings.MorningReport}" is missing from settings.`);

      if (!values.frontPageImagesReportId)
        toast.error(`Configuration "${Settings.FrontPageImagesReport}" is missing from settings.`);

      if (!values.topStoryAlertId)
        toast.error(`Configuration "${Settings.TopStoryAlert}" is missing from settings.`);
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
    values.editorUrl,
    values.subscriberUrl,
    values.defaultReportTemplateId,
    values.frontpageFilterId,
    values.excludeBylineIds,
    values.excludeSourceIds,
    values.morningReportId,
    values.frontPageImagesReportId,
    values.topStoryAlertId,
    values.frontpageImageMediaTypeId,
  ]);

  return values;
};
