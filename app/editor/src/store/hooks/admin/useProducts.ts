import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IProductModel, useApiAdminProducts } from 'tno-core';

interface IProductController {
  findAllProducts: () => Promise<IProductModel[]>;
  getProduct: (id: number) => Promise<IProductModel>;
  addProduct: (model: IProductModel) => Promise<IProductModel>;
  updateProduct: (model: IProductModel) => Promise<IProductModel>;
  deleteProduct: (model: IProductModel) => Promise<IProductModel>;
}

export const useProducts = (): [IAdminState, IProductController] => {
  const api = useApiAdminProducts();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllProducts: async () => {
        const response = await dispatch<IProductModel[]>('find-all-products', () =>
          api.findAllProducts(),
        );
        store.storeProducts(response.data);
        return response.data;
      },
      getProduct: async (id: number) => {
        const response = await dispatch<IProductModel>('get-product', () => api.getProduct(id));
        store.storeProducts((mediaTypes) =>
          mediaTypes.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addProduct: async (model: IProductModel) => {
        const response = await dispatch<IProductModel>('add-product', () => api.addProduct(model));
        store.storeProducts((mediaTypes) => {
          return [...mediaTypes, response.data].sort((a, b) =>
            a.name < b.name ? -1 : a.name > b.name ? 1 : 0,
          );
        });
        await lookup.getLookups();
        return response.data;
      },
      updateProduct: async (model: IProductModel) => {
        const response = await dispatch<IProductModel>('update-product', () =>
          api.updateProduct(model),
        );
        store.storeProducts((mediaTypes) =>
          mediaTypes.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteProduct: async (model: IProductModel) => {
        const response = await dispatch<IProductModel>('delete-product', () =>
          api.deleteProduct(model),
        );
        store.storeProducts((mediaTypes) => mediaTypes.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [dispatch, store, api, lookup],
  );

  return [state, controller];
};
