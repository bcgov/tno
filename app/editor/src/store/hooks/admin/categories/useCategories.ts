import { ICategoryModel, IPaged } from 'hooks';
import { useApiAdminCategories } from 'hooks/api-editor/admin/useApiAdminCategories';
import { ICategoryFilter } from 'hooks/api-editor/interfaces/ICategoryFilter';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface ICategoryController {
  findAllCategories: () => Promise<ICategoryModel[]>;
  findCategory: (filter: ICategoryFilter) => Promise<IPaged<ICategoryModel>>;
  getCategory: (id: number) => Promise<ICategoryModel>;
  addCategory: (model: ICategoryModel) => Promise<ICategoryModel>;
  updateCategory: (model: ICategoryModel) => Promise<ICategoryModel>;
  deleteCategory: (model: ICategoryModel) => Promise<ICategoryModel>;
}

export const useCategories = (): [IAdminState, ICategoryController] => {
  const api = useApiAdminCategories();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllCategories: async () => {
        const result = await dispatch<ICategoryModel[]>('find-all-categories', () =>
          api.findAllCategories(),
        );
        store.storeCategories(result);
        return result;
      },
      findCategory: async (filter: ICategoryFilter) => {
        const result = await dispatch<IPaged<ICategoryModel>>('find-categories', () =>
          api.findCategories(filter),
        );
        return result;
      },
      getCategory: async (id: number) => {
        const result = await dispatch<ICategoryModel>('get-category', () => api.getCategory(id));
        store.storeCategories(
          state.mediaTypes.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addCategory: async (model: ICategoryModel) => {
        const result = await dispatch<ICategoryModel>('add-category', () => api.addCategory(model));
        store.storeCategories([...state.mediaTypes, result]);
        return result;
      },
      updateCategory: async (model: ICategoryModel) => {
        const result = await dispatch<ICategoryModel>('update-category', () =>
          api.updateCategory(model),
        );
        store.storeCategories(
          state.mediaTypes.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteCategory: async (model: ICategoryModel) => {
        const result = await dispatch<ICategoryModel>('delete-category', () =>
          api.deleteCategory(model),
        );
        store.storeCategories(state.mediaTypes.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.mediaTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
