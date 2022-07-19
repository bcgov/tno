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
        const result = await dispatch<IActionModel[]>('find-all-tags', () => api.findAllActions());
        store.storeActions(result);
        return result;
      },
      findAction: async (filter: IActionFilter) => {
        const result = await dispatch<IPaged<IActionModel>>('find-tag', () =>
          api.findActions(filter),
        );
        return result;
      },
      getAction: async (id: number) => {
        const result = await dispatch<IActionModel>('get-tag', () => api.getAction(id));
        store.storeActions(
          state.actions.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addAction: async (model: IActionModel) => {
        const result = await dispatch<IActionModel>('add-tag', () => api.addAction(model));
        store.storeActions([...state.actions, result]);
        return result;
      },
      updateAction: async (model: IActionModel) => {
        const result = await dispatch<IActionModel>('update-tag', () => api.updateAction(model));
        store.storeActions(
          state.actions.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteAction: async (model: IActionModel) => {
        const result = await dispatch<IActionModel>('delete-tag', () => api.deleteAction(model));
        store.storeActions(state.actions.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.mediaTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
