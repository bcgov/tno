import { IProductModel, useApiAdminProducts } from 'hooks/api-editor';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IProductController {
  findAllProducts: () => Promise<IProductModel[]>;
  getProduct: (id: number) => Promise<IProductModel>;
  addProduct: (model: IProductModel) => Promise<IProductModel>;
  updateProduct: (model: IProductModel) => Promise<IProductModel>;
  deleteProduct: (model: IProductModel) => Promise<IProductModel>;
}

export const useProducts = (): [IAdminState, IProductController] => {
  const api = useApiAdminProducts();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllProducts: async () => {
        const result = await dispatch<IProductModel[]>('find-all-products', () =>
          api.findAllProducts(),
        );
        store.storeProducts(result);
        return result;
      },
      getProduct: async (id: number) => {
        const result = await dispatch<IProductModel>('get-product', () => api.getProduct(id));
        store.storeProducts(
          state.products.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addProduct: async (model: IProductModel) => {
        const result = await dispatch<IProductModel>('add-product', () => api.addProduct(model));
        store.storeProducts([...state.products, result]);
        return result;
      },
      updateProduct: async (model: IProductModel) => {
        const result = await dispatch<IProductModel>('update-product', () =>
          api.updateProduct(model),
        );
        store.storeProducts(
          state.products.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteProduct: async (model: IProductModel) => {
        const result = await dispatch<IProductModel>('delete-product', () =>
          api.deleteProduct(model),
        );
        store.storeProducts(state.products.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.products will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, store, api],
  );

  return [state, controller];
};
