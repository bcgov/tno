import { type AxiosResponse } from 'axios';
import React from 'react';
import { type ActionDelegate } from 'store';
import { type IWorkOrderState, useWorkOrderStore } from 'store/slices';
import {
  type IContentModel,
  type IPaged,
  type IWorkOrderFilter,
  type IWorkOrderModel,
  useApiEditorWorkOrders,
} from 'tno-core';

import { useAjaxWrapper } from '..';

interface IWorkOrderController {
  storeTranscriptFilter: (filter: IWorkOrderFilter | ActionDelegate<IWorkOrderFilter>) => void;
  findWorkOrders: (filter: IWorkOrderFilter) => Promise<AxiosResponse<IPaged<IWorkOrderModel>>>;
  updateWorkOrder: (workOrder: IWorkOrderModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  transcribe: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  autoClip: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  nlp: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  requestFile: (locationId: number, path: string) => Promise<AxiosResponse<IWorkOrderModel>>;
  ffmpeg: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
}

export const useWorkOrders = (): [IWorkOrderState, IWorkOrderController] => {
  const dispatch = useAjaxWrapper();
  const api = useApiEditorWorkOrders();
  const [state, store] = useWorkOrderStore();

  const controller = React.useMemo(
    () => ({
      findWorkOrders: async (filter: IWorkOrderFilter) => {
        const response = await dispatch<IPaged<IWorkOrderModel>>(
          'find-work-orders',
          async () => await api.findWorkOrders(filter),
          undefined,
          true,
        );
        return response;
      },
      updateWorkOrder: async (workOrder: IWorkOrderModel) => {
        return await dispatch(
          'update-work-order',
          async () => await api.updateWorkOrder(workOrder),
        );
      },
      transcribe: async (content: IContentModel) => {
        return await dispatch('transcribe-content', async () => await api.transcribe(content));
      },
      autoClip: async (content: IContentModel) => {
        return await dispatch('auto-clip-content', async () => await api.autoClip(content));
      },
      nlp: async (content: IContentModel) => {
        return await dispatch('nlp-content', async () => await api.nlp(content));
      },
      requestFile: async (locationId: number, path: string) => {
        return await dispatch('request-file', async () => await api.requestFile(locationId, path));
      },
      ffmpeg: async (content: IContentModel) => {
        return await dispatch('ffmpeg-content', async () => await api.ffmpeg(content));
      },
    }),
    [api, dispatch],
  );

  return [state, { ...controller, ...store }];
};
