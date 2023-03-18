import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { ITopicScoreRuleModel, useApiAdminTopicScoreRules } from 'tno-core';

interface ITopicScoreRuleController {
  findAllTopicScoreRules: () => Promise<ITopicScoreRuleModel[]>;
  getTopicScoreRule: (id: number) => Promise<ITopicScoreRuleModel>;
  addTopicScoreRule: (model: ITopicScoreRuleModel) => Promise<ITopicScoreRuleModel>;
  updateTopicScoreRule: (model: ITopicScoreRuleModel) => Promise<ITopicScoreRuleModel>;
  updateTopicScoreRules: (models: ITopicScoreRuleModel[]) => Promise<ITopicScoreRuleModel[]>;
  deleteTopicScoreRule: (model: ITopicScoreRuleModel) => Promise<ITopicScoreRuleModel>;
}

export const useTopicScoreRules = (): [IAdminState, ITopicScoreRuleController] => {
  const api = useApiAdminTopicScoreRules();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllTopicScoreRules: async () => {
        const response = await dispatch<ITopicScoreRuleModel[]>('find-all-topic-score-rules', () =>
          api.findAllTopicScoreRules(),
        );
        store.storeTopicScoreRules(response.data);
        return response.data;
      },
      getTopicScoreRule: async (id: number) => {
        const response = await dispatch<ITopicScoreRuleModel>('get-topic-score-rule', () =>
          api.getTopicScoreRule(id),
        );
        store.storeTopicScoreRules(
          state.rules.map((t) => {
            if (t.id === response.data.id) return response.data;
            return t;
          }),
        );
        return response.data;
      },
      addTopicScoreRule: async (model: ITopicScoreRuleModel) => {
        const response = await dispatch<ITopicScoreRuleModel>('add-topic-score-rule', () =>
          api.addTopicScoreRule(model),
        );
        var items = [...state.rules];
        items.splice(model.sortOrder, 0, response.data);
        store.storeTopicScoreRules(items);
        return response.data;
      },
      updateTopicScoreRule: async (model: ITopicScoreRuleModel) => {
        const response = await dispatch<ITopicScoreRuleModel>('update-topic-score-rule', () =>
          api.updateTopicScoreRule(model),
        );
        store.storeTopicScoreRules(
          state.rules.map((t) => {
            if (t.id === response.data.id) return response.data;
            return t;
          }),
        );
        return response.data;
      },
      updateTopicScoreRules: async (models: ITopicScoreRuleModel[]) => {
        const response = await dispatch<ITopicScoreRuleModel[]>('update-topic-score-rules', () =>
          api.updateTopicScoreRules(models),
        );
        store.storeTopicScoreRules(response.data);
        return response.data;
      },
      deleteTopicScoreRule: async (model: ITopicScoreRuleModel) => {
        const response = await dispatch<ITopicScoreRuleModel>('delete-topic-score-rule', () =>
          api.deleteTopicScoreRule(model),
        );
        store.storeTopicScoreRules(state.rules.filter((t) => t.id !== response.data.id));
        return response.data;
      },
    }),
    [api, dispatch, state.rules, store],
  );

  return [state, controller];
};
