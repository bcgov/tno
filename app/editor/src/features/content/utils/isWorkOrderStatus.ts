import { IWorkOrderModel, WorkOrderStatusName, WorkOrderTypeName } from 'hooks';

/**
 * Determines if there are any work orders of the specified 'type' in the specified 'status'.
 * @param workOrders An array of work orders.
 * @param type The type of work order to search for.
 * @param status An array of valid status.
 * @returns True if a work order is found.
 */
export const isWorkOrderStatus = (
  workOrders: IWorkOrderModel[] | undefined,
  type: WorkOrderTypeName,
  status: WorkOrderStatusName[],
) => {
  return workOrders?.some((i) => i.workType === type && status.includes(i.status)) ?? false;
};
