import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IActionFilter, IActionModel, IPaged, useApiAdminActions } from 'tno-core';

interface IActionController {
  findAllActions: () => Promise<IActionModel[]>;
  findAction: (filter: IActionFilter) => Promise<IPaged<IActionModel>>;
  getAction: (id: number) => Promise<IActionModel>;
  addAction: (model: IActionModel) => Promise<IActionModel>;
  updateAction: (model: IActionModel) => Promise<IActionModel>;
  deleteAction: (model: IActionModel) => Promise<IActionModel>;
}

export const useActions = (): [IAdminState, IActionController] => {
  const api = useApiAdminActions();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllActions: async () => {
        const response = await dispatch<IActionModel[]>('find-all-actions', () =>
          api.findAllActions(),
        );
        store.storeActions(response.data);
        return response.data;
      },
      findAction: async (filter: IActionFilter) => {
        const response = await dispatch<IPaged<IActionModel>>('find-action', () =>
          api.findActions(filter),
        );
        return response.data;
      },
      getAction: async (id: number) => {
        const response = await dispatch<IActionModel>('get-action', () => api.getAction(id));
        store.storeActions((actions) =>
          actions.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addAction: async (model: IActionModel) => {
        const response = await dispatch<IActionModel>('add-action', () => api.addAction(model));
        store.storeActions((actions) => [...actions, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateAction: async (model: IActionModel) => {
        const response = await dispatch<IActionModel>('update-action', () =>
          api.updateAction(model),
        );
        store.storeActions((actions) =>
          actions.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteAction: async (model: IActionModel) => {
        const response = await dispatch<IActionModel>('delete-action', () =>
          api.deleteAction(model),
        );
        store.storeActions((actions) => actions.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
