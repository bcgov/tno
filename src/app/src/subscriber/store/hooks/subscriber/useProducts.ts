import React from 'react';

import { IProductModel, IUserProductModel, useApiSubscriberProducts } from '@/shared';
import { useAjaxWrapper } from '@/subscriber/store/hooks';

interface IProductController {
  getProducts: () => Promise<IProductModel[]>;
  toggleSubscription: (model: IUserProductModel) => Promise<IUserProductModel>;
}

export const useProducts = (): [IProductController] => {
  const api = useApiSubscriberProducts();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getProducts: async () => {
        const response = await dispatch<IProductModel[]>('get-products', () => api.getProducts());
        return response.data;
      },
      toggleSubscription: async (model: IUserProductModel) => {
        const response = await dispatch<IUserProductModel>('toggle-product-subscription', () =>
          api.toggleSubscription(model),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
