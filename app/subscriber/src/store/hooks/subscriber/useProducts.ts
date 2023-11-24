import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IProductSubscriberModel, useApiSubscriberProducts } from 'tno-core';

interface IProductController {
  getProducts: () => Promise<IProductSubscriberModel[]>;
  toggleSubscription: (model: IProductSubscriberModel) => Promise<IProductSubscriberModel>;
}

export const useProducts = (): [IProductController] => {
  const api = useApiSubscriberProducts();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getProducts: async () => {
        const response = await dispatch<IProductSubscriberModel[]>('get-products', () =>
          api.getProducts(),
        );
        return response.data;
      },
      toggleSubscription: async (model: IProductSubscriberModel) => {
        const response = await dispatch<IProductSubscriberModel>(
          'toggle-product-subscription',
          () => api.toggleSubscription(model),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
