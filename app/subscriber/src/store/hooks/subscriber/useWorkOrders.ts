import { AxiosResponse } from 'axios';
import React from 'react';
import {
  IContentModel,
  IPaged,
  IWorkOrderFilter,
  IWorkOrderModel,
  useApiSubscriberWorkOrders,
} from 'tno-core';

import { useAjaxWrapper } from '..';

interface IWorkOrderController {
  findWorkOrders: (filter: IWorkOrderFilter) => Promise<IPaged<IWorkOrderModel>>;
  transcribe: (content: IContentModel) => Promise<AxiosResponse<IWorkOrderModel>>;
  transcribeAnonymous: (contentId: number, uid: number) => Promise<AxiosResponse<any>>;
}

export const useWorkOrders = (): [any, IWorkOrderController] => {
  const dispatch = useAjaxWrapper();
  const api = useApiSubscriberWorkOrders();

  const controller = React.useMemo(
    () => ({
      findWorkOrders: async (filter: IWorkOrderFilter) => {
        const response = await dispatch<IPaged<IWorkOrderModel>>('find-work-orders', () =>
          api.findWorkOrders(filter),
        );
        return response.data as IPaged<IWorkOrderModel>;
      },
      transcribe: async (content: IContentModel) => {
        return (await dispatch('transcribe-content', () =>
          api.transcribe(content),
        )) as AxiosResponse<IWorkOrderModel>;
      },
      transcribeAnonymous: async (contentId: number, uid: number) => {
        const response = await dispatch<any>('transcribe-content-anonymous', () =>
          api.transcribeAnonymous(contentId, uid),
        );
        return response.data as any;
      },
    }),
    [api, dispatch],
  );

  return [{}, controller];
};
