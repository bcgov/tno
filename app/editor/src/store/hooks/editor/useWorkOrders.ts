import { AxiosResponse } from 'axios';
import React from 'react';
import {
  IContentModel,
  IPaged,
  IWorkOrderFilter,
  IWorkOrderModel,
  useApiEditorWorkOrders,
} from 'tno-core';

import { useAjaxWrapper } from '..';

interface IWorkOrderController {
  findWorkOrders: (filter: IWorkOrderFilter) => Promise<AxiosResponse<IPaged<IWorkOrderModel>>>;
  transcribe: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  nlp: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  requestFile: (locationId: number, path: string) => Promise<AxiosResponse<IWorkOrderModel>>;
}

export const useWorkOrders = (): [any, IWorkOrderController] => {
  const dispatch = useAjaxWrapper();
  const api = useApiEditorWorkOrders();

  const controller = React.useMemo(
    () => ({
      findWorkOrders: async (filter: IWorkOrderFilter) => {
        const response = await dispatch<IPaged<IWorkOrderModel>>(
          'find-work-orders',
          () => api.findWorkOrders(filter),
          undefined,
          true,
        );
        return response;
      },
      transcribe: async (content: IContentModel) => {
        return await dispatch('transcribe-content', () => api.transcribe(content));
      },
      nlp: async (content: IContentModel) => {
        return await dispatch('nlp-content', () => api.nlp(content));
      },
      requestFile: async (locationId: number, path: string) => {
        return await dispatch('request-file', () => api.requestFile(locationId, path));
      },
    }),
    [api, dispatch],
  );

  return [{}, controller];
};
