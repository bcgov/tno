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
        const response = await dispatch<ICategoryModel[]>('find-all-categories', () =>
          api.findAllCategories(),
        );
        store.storeCategories(response.data);
        return response.data;
      },
      findCategory: async (filter: ICategoryFilter) => {
        const response = await dispatch<IPaged<ICategoryModel>>('find-categories', () =>
          api.findCategories(filter),
        );
        return response.data;
      },
      getCategory: async (id: number) => {
        const response = await dispatch<ICategoryModel>('get-category', () => api.getCategory(id));
        store.storeCategories(
          state.categories.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addCategory: async (model: ICategoryModel) => {
        const response = await dispatch<ICategoryModel>('add-category', () =>
          api.addCategory(model),
        );
        store.storeCategories([...state.categories, response.data]);
        return response.data;
      },
      updateCategory: async (model: ICategoryModel) => {
        const response = await dispatch<ICategoryModel>('update-category', () =>
          api.updateCategory(model),
        );
        store.storeCategories(
          state.categories.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      deleteCategory: async (model: ICategoryModel) => {
        const response = await dispatch<ICategoryModel>('delete-category', () =>
          api.deleteCategory(model),
        );
        store.storeCategories(state.categories.filter((ds) => ds.id !== response.data.id));
        return response.data;
      },
    }),
    // The state.categories will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
