import {
  type ITopicRescoreJobModel,
  type ITopicRescorePreviewModel,
  type ITopicRescoreRequestModel,
  type ITopicScoreSourceModel,
  type ITopicScoreTestRequestModel,
  type ITopicScoreTestResultModel,
} from 'features/admin/topic-score-rules/interfaces';
import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { type ITopicScoreRuleModel } from 'tno-core';

import { useApiAdminTopicScores } from './useApiAdminTopicScores';

export interface ITopicScoreController {
  findSources: () => Promise<ITopicScoreSourceModel[]>;
  updateSourceDefaultScore: (model: ITopicScoreSourceModel) => Promise<ITopicScoreSourceModel>;
  removeSource: (sourceId: number) => Promise<void>;
  findRules: (sourceId: number) => Promise<ITopicScoreRuleModel[]>;
  findSections: (sourceId: number) => Promise<string[]>;
  reorderRules: (sourceId: number, ruleIds: number[]) => Promise<ITopicScoreRuleModel[]>;
  addRule: (model: ITopicScoreRuleModel) => Promise<ITopicScoreRuleModel>;
  updateRule: (model: ITopicScoreRuleModel) => Promise<ITopicScoreRuleModel>;
  deleteRule: (model: ITopicScoreRuleModel) => Promise<ITopicScoreRuleModel>;
  test: (model: ITopicScoreTestRequestModel) => Promise<ITopicScoreTestResultModel>;
  previewRescore: (model: ITopicRescoreRequestModel) => Promise<ITopicRescorePreviewModel>;
  rescore: (model: ITopicRescoreRequestModel) => Promise<ITopicRescoreJobModel>;
  findRescoreJobs: () => Promise<ITopicRescoreJobModel[]>;
  findRescoreJob: (id: number) => Promise<ITopicRescoreJobModel>;
}

/**
 * Topic score administration: sources, per-source rules, the rule tester, and bulk rescore.
 * @returns Controller.
 */
export const useTopicScores = (): ITopicScoreController => {
  const api = useApiAdminTopicScores();
  const dispatch = useAjaxWrapper();
  const [, lookup] = useLookup();

  return React.useMemo(
    () => ({
      findSources: async () =>
        (await dispatch('find-topic-score-sources', () => api.findSources())).data,
      updateSourceDefaultScore: async (model: ITopicScoreSourceModel) =>
        (await dispatch('update-topic-default-score', () => api.updateSourceDefaultScore(model)))
          .data,
      removeSource: async (sourceId: number) => {
        await dispatch('remove-topic-score-source', () => api.removeSource(sourceId));
        await lookup.getLookups();
      },
      findRules: async (sourceId: number) =>
        (await dispatch('find-topic-score-rules', () => api.findRules(sourceId))).data,
      findSections: async (sourceId: number) =>
        (
          await dispatch(
            'find-topic-score-sections',
            () => api.findSections(sourceId),
            undefined,
            true,
          )
        ).data,
      reorderRules: async (sourceId: number, ruleIds: number[]) =>
        (await dispatch('reorder-topic-score-rules', () => api.reorderRules(sourceId, ruleIds)))
          .data,
      addRule: async (model: ITopicScoreRuleModel) =>
        (await dispatch('add-topic-score-rule', () => api.addRule(model))).data,
      updateRule: async (model: ITopicScoreRuleModel) =>
        (await dispatch('update-topic-score-rule', () => api.updateRule(model))).data,
      deleteRule: async (model: ITopicScoreRuleModel) =>
        (await dispatch('delete-topic-score-rule', () => api.deleteRule(model))).data,
      test: async (model: ITopicScoreTestRequestModel) =>
        (await dispatch('test-topic-score', () => api.test(model))).data,
      previewRescore: async (model: ITopicRescoreRequestModel) =>
        (await dispatch('preview-topic-rescore', () => api.previewRescore(model))).data,
      rescore: async (model: ITopicRescoreRequestModel) =>
        (await dispatch('topic-rescore', () => api.rescore(model))).data,
      findRescoreJobs: async () =>
        (await dispatch('find-topic-rescore-jobs', () => api.findRescoreJobs(), undefined, true))
          .data,
      findRescoreJob: async (id: number) =>
        (await dispatch('find-topic-rescore-job', () => api.findRescoreJob(id), undefined, true))
          .data,
    }),
    [api, dispatch, lookup],
  );
};
