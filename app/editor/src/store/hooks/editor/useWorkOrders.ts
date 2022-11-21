import { AxiosResponse } from 'axios';
import {
  IContentModel,
  IPaged,
  IWorkOrderFilter,
  IWorkOrderModel,
  useApiWorkOrders,
} from 'hooks/api-editor';
import React from 'react';

import { useAjaxWrapper } from '..';

interface IWorkOrderController {
  findWorkOrders: (filter: IWorkOrderFilter) => Promise<AxiosResponse<IPaged<IWorkOrderModel>>>;
  transcribe: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  nlp: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
}

export const useWorkOrders = (): [any, IWorkOrderController] => {
  const dispatch = useAjaxWrapper();
  const api = useApiWorkOrders();

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
    }),
    [api, dispatch],
  );

  return [{}, controller];
};
