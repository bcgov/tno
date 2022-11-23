import { IWorkOrderListFilter } from 'features/work-orders/interfaces/IWorkOrderListFilter';
import { IPaged } from 'hooks';

import { IWorkOrderModel } from './IWorkOrderModel';

export interface IWorkOrderState {
  workOrderFilter: IWorkOrderListFilter;
  workOrders: IPaged<IWorkOrderModel>;
}
