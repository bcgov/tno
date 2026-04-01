import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';
import { type ILLMModel, useApiAdminLLMs } from 'tno-core';

interface ILLMController {
  findAllLLMs: () => Promise<ILLMModel[]>;
  getLLM: (id: number) => Promise<ILLMModel>;
  addLLM: (model: ILLMModel) => Promise<ILLMModel>;
  updateLLM: (model: ILLMModel) => Promise<ILLMModel>;
  deleteLLM: (model: ILLMModel) => Promise<ILLMModel>;
}

export const useLLMs = (): [IAdminState, ILLMController] => {
  const api = useApiAdminLLMs();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllLLMs: async () => {
        const response = await dispatch<ILLMModel[]>(
          'find-all-llms',
          async () => await api.findAllLLMs(),
        );
        store.storeLLMs(response.data);
        return response.data;
      },
      getLLM: async (id: number) => {
        const response = await dispatch<ILLMModel>('get-llm', async () => await api.getLLM(id));
        store.storeLLMs((llms) =>
          llms.map((l) => {
            if (l.id === response.data.id) return response.data;
            return l;
          }),
        );
        return response.data;
      },
      addLLM: async (model: ILLMModel) => {
        const response = await dispatch<ILLMModel>('add-llm', async () => await api.addLLM(model));
        store.storeLLMs((llms) => [...llms, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateLLM: async (model: ILLMModel) => {
        const response = await dispatch<ILLMModel>(
          'update-llm',
          async () => await api.updateLLM(model),
        );
        store.storeLLMs((llms) =>
          llms.map((l) => {
            if (l.id === response.data.id) return response.data;
            return l;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteLLM: async (model: ILLMModel) => {
        const response = await dispatch<ILLMModel>(
          'delete-llm',
          async () => await api.deleteLLM(model),
        );
        store.storeLLMs((llms) => llms.filter((l) => l.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [dispatch, store, api, lookup],
  );

  return [state, controller];
};
