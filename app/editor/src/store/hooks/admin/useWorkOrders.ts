import { IWorkOrderListFilter } from 'features/admin/work-orders/interfaces/IWorkOrderListFilter';
import { useAdmin } from 'hooks';
import {
  IPaged,
  IWorkOrderFilter,
  IWorkOrderModel,
  useApiAdminWorkOrders,
  useApiWorkOrders,
} from 'hooks/api-editor';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface IWorkOrderController {
  findWorkOrders: (filter: IWorkOrderFilter) => Promise<IPaged<IWorkOrderModel>>;
  getWorkOrder: (id: number) => Promise<IWorkOrderModel>;
  addWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  updateWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  deleteWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  storeFilter: (filter: IWorkOrderListFilter) => void;
}

export const useWorkOrders = (): [IAdminState, IWorkOrderController] => {
  const adminApi = useApiAdminWorkOrders();
  const api = useApiWorkOrders();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const isAdmin = useAdmin();

  const controller = React.useMemo(
    () => ({
      findWorkOrders: async (filter: IWorkOrderFilter) => {
        const response = await dispatch<IPaged<IWorkOrderModel>>('find-work-orders', () =>
          isAdmin ? adminApi.findWorkOrders(filter) : api.findWorkOrders(filter),
        );
        store.storeWorkOrders(response.data);
        return response.data;
      },
      getWorkOrder: async (id: number) => {
        const response = await dispatch<IWorkOrderModel>('get-work-order', () =>
          adminApi.getWorkOrder(id),
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
          adminApi.addWorkOrder(model),
        );
        store.storeWorkOrders({
          ...state.workOrders,
          items: [response.data, ...state.workOrders.items],
        });
        return response.data;
      },
      updateWorkOrder: async (model: IWorkOrderModel) => {
        const response = await dispatch<IWorkOrderModel>('update-work-order', () =>
          adminApi.updateWorkOrder(model),
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
          adminApi.deleteWorkOrder(model),
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
    [adminApi, dispatch, store],
  );

  return [state, controller];
};
