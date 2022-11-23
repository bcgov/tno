import { AxiosResponse } from 'axios';
import { IContentModel, IPaged, useApiWorkOrders } from 'hooks/api-editor';
import React from 'react';
import { IWorkOrderProps, useWorkOrderStore } from 'store/slices';
import {
  IWorkOrderFilter,
  IWorkOrderListFilter,
  IWorkOrderModel,
} from 'store/slices/workorder/interfaces';

import { useAjaxWrapper } from '..';

interface IWorkOrderController {
  findWorkOrders: (filter: IWorkOrderFilter) => Promise<IPaged<IWorkOrderModel>>;
  getWorkOrder: (id: number) => Promise<IWorkOrderModel>;
  addWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  updateWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  deleteWorkOrder: (model: IWorkOrderModel) => Promise<IWorkOrderModel>;
  storeFilter: (filter: IWorkOrderListFilter) => void;
  transcribe: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  nlp: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
}

export const useWorkOrders = (props?: IWorkOrderProps): [any, IWorkOrderController] => {
  const dispatch = useAjaxWrapper();
  const api = useApiWorkOrders();
  const [state, store] = useWorkOrderStore(props);

  const controller = React.useMemo(
    () => ({
      findWorkOrders: async (filter: IWorkOrderFilter) => {
        const response = await dispatch<IPaged<IWorkOrderModel>>('find-work-orders', () =>
          api.findWorkOrders(filter),
        );
        store.storeWorkOrders(response.data);
        return response.data;
      },
      transcribe: async (content: IContentModel) => {
        return await dispatch('transcribe-content', () => api.transcribe(content));
      },
      nlp: async (content: IContentModel) => {
        return await dispatch('nlp-content', () => api.nlp(content));
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dispatch, api, store],
  );

  return [state, controller];
};
