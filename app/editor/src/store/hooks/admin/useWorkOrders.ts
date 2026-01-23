import { type IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';
import {
  type IPaged,
  type IWorkOrderFilter,
  type IWorkOrderModel,
  useApiAdminWorkOrders,
} from 'tno-core';

interface IWorkOrderController {
  findWorkOrders: (filter: IWorkOrderFilter) => Promise<IPaged<IWorkOrderModel>>;
  getWorkOrder: (id: number) => Promise<IWorkOrderModel>;
  addWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  updateWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  deleteWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  storeFilter: (filter: IWorkOrderListFilter) => void;
}

export const useWorkOrders = (): [IAdminState, IWorkOrderController] => {
  const api = useApiAdminWorkOrders();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findWorkOrders: async (filter: IWorkOrderFilter) => {
        const response = await dispatch<IPaged<IWorkOrderModel>>(
          'find-work-orders',
          async () => await api.findWorkOrders(filter),
        );
        store.storeWorkOrders(response.data);
        return response.data;
      },
      getWorkOrder: async (id: number) => {
        const response = await dispatch<IWorkOrderModel>(
          'get-work-order',
          async () => await api.getWorkOrder(id),
        );
        store.storeWorkOrders((workOrders) => ({
          ...workOrders,
          items: workOrders.items.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        }));
        return response.data;
      },
      addWorkOrder: async (model: IWorkOrderModel) => {
        const response = await dispatch<IWorkOrderModel>(
          'add-work-order',
          async () => await api.addWorkOrder(model),
        );
        store.storeWorkOrders((workOrders) => ({
          ...workOrders,
          items: [response.data, ...workOrders.items],
        }));
        return response.data;
      },
      updateWorkOrder: async (model: IWorkOrderModel) => {
        const response = await dispatch<IWorkOrderModel>(
          'update-work-order',
          async () => await api.updateWorkOrder(model),
        );
        store.storeWorkOrders((workOrders) => ({
          ...workOrders,
          items: workOrders.items.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        }));
        return response.data;
      },
      deleteWorkOrder: async (model: IWorkOrderModel) => {
        const response = await dispatch<IWorkOrderModel>(
          'delete-work-order',
          async () => await api.deleteWorkOrder(model),
        );
        store.storeWorkOrders((workOrders) => ({
          ...workOrders,
          items: workOrders.items.filter((ds) => ds.id !== response.data.id),
        }));
        return response.data;
      },
      storeFilter: store.storeWorkOrderFilter,
    }),
    [api, dispatch, store],
  );

  return [state, controller];
};
