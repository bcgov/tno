import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IReportSectionSettingsModel } from 'tno-core';

import {
  IReportAIPromptChatRequestModel,
  IReportAIPromptChatResultModel,
  IReportAISectionPromptModel,
  IReportAISectionTestResultModel,
} from './interfaces';
import { useApiSubscriberReportAI } from './useApiSubscriberReportAI';

export interface IReportAIController {
  /** The requests the section would send with the given (possibly unsaved) settings. */
  describeSection: (
    reportId: number,
    sectionId: number,
    settings: IReportSectionSettingsModel,
  ) => Promise<IReportAISectionPromptModel>;
  /** The requests the section sends for the instance with its saved settings. */
  describeInstanceSection: (
    instanceId: number,
    sectionId: number,
  ) => Promise<IReportAISectionPromptModel>;
  /** Start a test run of the section with the given settings. */
  testSection: (
    reportId: number,
    sectionId: number,
    settings: IReportSectionSettingsModel,
  ) => Promise<IReportAISectionTestResultModel>;
  /** Fetch a test run; undefined when it is unknown or has expired. */
  getTest: (
    reportId: number,
    sectionId: number,
    testId: string,
  ) => Promise<IReportAISectionTestResultModel | undefined>;
  /** Ask the section's model how to change the prompt. */
  chat: (
    reportId: number,
    sectionId: number,
    request: IReportAIPromptChatRequestModel,
  ) => Promise<IReportAIPromptChatResultModel>;
}

export const useReportAI = (): IReportAIController => {
  const api = useApiSubscriberReportAI();
  const dispatch = useAjaxWrapper();

  return React.useMemo(
    () => ({
      describeSection: async (reportId, sectionId, settings) => {
        const response = await dispatch<IReportAISectionPromptModel>('ai-prompt', () =>
          api.describeSection(reportId, sectionId, settings),
        );
        return response.data;
      },
      describeInstanceSection: async (instanceId, sectionId) => {
        const response = await dispatch<IReportAISectionPromptModel>('ai-prompt', () =>
          api.describeInstanceSection(instanceId, sectionId),
        );
        return response.data;
      },
      testSection: async (reportId, sectionId, settings) => {
        const response = await dispatch<IReportAISectionTestResultModel>('ai-test', () =>
          api.testSection(reportId, sectionId, settings),
        );
        return response.data;
      },
      getTest: async (reportId, sectionId, testId) => {
        // Polled in the background: no spinner and no error toast.
        const response = await dispatch<IReportAISectionTestResultModel | undefined>(
          'ai-test-status',
          () => api.getTest(reportId, sectionId, testId),
          undefined,
          true,
          true,
        );
        return response.data && typeof response.data === 'object' ? response.data : undefined;
      },
      chat: async (reportId, sectionId, request) => {
        const response = await dispatch<IReportAIPromptChatResultModel>('ai-chat', () =>
          api.chat(reportId, sectionId, request),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );
};
