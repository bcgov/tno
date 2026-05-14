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
        return response as AxiosResponse<IPaged<IWorkOrderModel>, any>;
      },
      updateWorkOrder: async (workOrder: IWorkOrderModel) => {
        return await dispatch<IWorkOrderModel>(
          'update-work-order',
          async () => await api.updateWorkOrder(workOrder),
        );
      },
      transcribe: async (content: IContentModel) => {
        return (await dispatch(
          'transcribe-content',
          async () => await api.transcribe(content),
        )) as AxiosResponse<IWorkOrderModel, any>;
      },
      autoClip: async (content: IContentModel) => {
        return (await dispatch(
          'auto-clip-content',
          async () => await api.autoClip(content),
        )) as AxiosResponse<IWorkOrderModel, any>;
      },
      nlp: async (content: IContentModel) => {
        return (await dispatch('nlp-content', async () => await api.nlp(content))) as AxiosResponse<
          IWorkOrderModel,
          any
        >;
      },
      requestFile: async (locationId: number, path: string) => {
        return (await dispatch(
          'request-file',
          async () => await api.requestFile(locationId, path),
        )) as AxiosResponse<any>;
      },
      ffmpeg: async (content: IContentModel) => {
        return (await dispatch(
          'ffmpeg-content',
          async () => await api.ffmpeg(content),
        )) as AxiosResponse<IWorkOrderModel, any>;
      },
    }),
    [api, dispatch],
  );

  return [state, { ...controller, ...store }];
};
