import { AxiosResponse } from 'axios';
import { IContentModel, IWorkOrderModel, useApiWorkOrders } from 'hooks/api-editor';
import React from 'react';

import { useApiDispatcher } from '..';

interface IWorkOrderController {
  transcribe: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  nlp: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
}

export const useWorkOrders = (): [any, IWorkOrderController] => {
  const dispatch = useApiDispatcher();
  const api = useApiWorkOrders();

  const controller = React.useRef({
    transcribe: async (content: IContentModel) => {
      return await dispatch('transcribe-content', () => api.transcribe(content));
    },
    nlp: async (content: IContentModel) => {
      return await dispatch('nlp-content', () => api.nlp(content));
    },
  }).current;

  return [{}, controller];
};
