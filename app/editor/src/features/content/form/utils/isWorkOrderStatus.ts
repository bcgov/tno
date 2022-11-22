import { WorkOrderStatusName, WorkOrderTypeName } from 'hooks';

import { IContentForm } from '../interfaces';

/**
 * Determines if there are any work orders of the specified 'type' in the specified 'status'.
 * @param content Content with work orders to validate.
 * @param type The type of work order to search for.
 * @param status An array of valid status.
 * @returns True if a work order is found.
 */
export const isWorkOrderStatus = (
  content: IContentForm,
  type: WorkOrderTypeName,
  status: WorkOrderStatusName[],
) => {
  return content.workOrders?.some((i) => i.workType === type && status.includes(i.status)) ?? false;
};

export const isWorkOrderActive = (content: IContentForm, type: WorkOrderTypeName) => {
  return isWorkOrderStatus(content, type, [
    WorkOrderStatusName.Submitted,
    WorkOrderStatusName.InProgress,
  ]);
};

export const isWorkOrderComplete = (content: IContentForm, type: WorkOrderTypeName) => {
  return isWorkOrderStatus(content, type, [WorkOrderStatusName.Completed]);
};

export const isWorkOrderFailed = (content: IContentForm, type: WorkOrderTypeName) => {
  return isWorkOrderStatus(content, type, [WorkOrderStatusName.Failed]);
};

export const isWorkOrderCancelled = (content: IContentForm, type: WorkOrderTypeName) => {
  return isWorkOrderStatus(content, type, [WorkOrderStatusName.Cancelled]);
};
