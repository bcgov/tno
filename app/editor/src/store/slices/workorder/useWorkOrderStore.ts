import { IWorkOrderListFilter } from 'features/work-orders/interfaces/IWorkOrderListFilter';
import { IPaged } from 'hooks/api-editor';
import React from 'react';
import { useAppDispatch, useAppSelector } from 'store';

import { storeWorkOrderFilter, storeWorkOrders } from '.';
import { IWorkOrderModel, IWorkOrderState } from './interfaces';

export interface IWorkOrderProps {}

export interface IWorkorderStore {
  storeWorkOrderFilter: (filter: IWorkOrderListFilter) => void;
  storeWorkOrders: (users: IPaged<IWorkOrderModel>) => void;
}

export const useWorkOrderStore = (props?: IWorkOrderProps): [IWorkOrderState, IWorkorderStore] => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.workOrders);

  const controller = React.useMemo(
    () => ({
      storeWorkOrderFilter: (filter: IWorkOrderListFilter) => {
        dispatch(storeWorkOrderFilter(filter));
      },
      storeWorkOrders: (workOrders: IPaged<IWorkOrderModel>) => {
        dispatch(storeWorkOrders(workOrders));
      },
    }),
    [dispatch],
  );

  return [state, controller];
};
