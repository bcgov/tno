import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IPaged, IWorkOrderFilter, IWorkOrderModel, useApiAdminWorkOrders } from 'tno-core';

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
        const response = await dispatch<IPaged<IWorkOrderModel>>('find-work-orders', () =>
          api.findWorkOrders(filter),
        );
        store.storeWorkOrders(response.data);
        return response.data;
      },
      getWorkOrder: async (id: number) => {
        const response = await dispatch<IWorkOrderModel>('get-work-order', () =>
          api.getWorkOrder(id),
        );
        store.storeWorkOrders({
          ...state.workOrders,
          items: state.workOrders.items.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        });
        return response.data;
      },
      addWorkOrder: async (model: IWorkOrderModel) => {
        const response = await dispatch<IWorkOrderModel>('add-work-order', () =>
          api.addWorkOrder(model),
        );
        store.storeWorkOrders({
          ...state.workOrders,
          items: [response.data, ...state.workOrders.items],
        });
        return response.data;
      },
      updateWorkOrder: async (model: IWorkOrderModel) => {
        const response = await dispatch<IWorkOrderModel>('update-work-order', () =>
          api.updateWorkOrder(model),
        );
        store.storeWorkOrders({
          ...state.workOrders,
          items: state.workOrders.items.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        });
        return response.data;
      },
      deleteWorkOrder: async (model: IWorkOrderModel) => {
        const response = await dispatch<IWorkOrderModel>('delete-work-order', () =>
          api.deleteWorkOrder(model),
        );
        store.storeWorkOrders({
          ...state.workOrders,
          items: state.workOrders.items.filter((ds) => ds.id !== response.data.id),
        });
        return response.data;
      },
      storeFilter: store.storeWorkOrderFilter,
    }),
    // The state.workOrders will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
