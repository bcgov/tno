import { IActionFilter, IActionModel, IPaged, useApiAdminActions } from 'hooks';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

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
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllActions: async () => {
        const response = await dispatch<IActionModel[]>('find-all-tags', () =>
          api.findAllActions(),
        );
        store.storeActions(response.data);
        return response.data;
      },
      findAction: async (filter: IActionFilter) => {
        const response = await dispatch<IPaged<IActionModel>>('find-tag', () =>
          api.findActions(filter),
        );
        return response.data;
      },
      getAction: async (id: number) => {
        const response = await dispatch<IActionModel>('get-tag', () => api.getAction(id));
        store.storeActions(
          state.actions.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addAction: async (model: IActionModel) => {
        const response = await dispatch<IActionModel>('add-tag', () => api.addAction(model));
        store.storeActions([...state.actions, response.data]);
        return response.data;
      },
      updateAction: async (model: IActionModel) => {
        const response = await dispatch<IActionModel>('update-tag', () => api.updateAction(model));
        store.storeActions(
          state.actions.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteAction: async (model: IActionModel) => {
        const response = await dispatch<IActionModel>('delete-tag', () => api.deleteAction(model));
        store.storeActions(state.actions.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.mediaTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
