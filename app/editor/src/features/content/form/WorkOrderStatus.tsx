import { IWorkOrderModel, useTooltips, WorkOrderStatusName, WorkOrderTypeName } from 'hooks';
import { FaCheckCircle, FaExclamationCircle, FaTimesCircle } from 'react-icons/fa';
import { Spinner, SpinnerVariant } from 'tno-core';

import { findWorkOrder } from '../utils';

export interface ITranscriptionStatusProps {
  /** An array of work orders. */
  workOrders: IWorkOrderModel[] | undefined;
  /** The type of work order. */
  type: WorkOrderTypeName;
}

/**
 * Displays a status icon for the specified work order type.
 * @param param0 Component properties
 * @returns An icon or nothing if there is no work order associated with the specified type.
 */
export const WorkOrderStatus: React.FC<ITranscriptionStatusProps> = ({ workOrders, type }) => {
  const workOrder = findWorkOrder(workOrders, type);
  useTooltips();

  if (!workOrder) return <></>;

  switch (workOrder?.status) {
    case WorkOrderStatusName.Cancelled:
      return (
        <FaTimesCircle className="spinner" data-for="main-tooltip" data-tip={workOrder.status} />
      );
    case WorkOrderStatusName.Completed:
      return (
        <FaCheckCircle className="spinner" data-for="main-tooltip" data-tip={workOrder.status} />
      );
    case WorkOrderStatusName.Failed:
      return (
        <FaExclamationCircle
          className="spinner"
          data-for="main-tooltip"
          data-tip={workOrder.status}
        />
      );
    default:
      return (
        <Spinner
          variant={SpinnerVariant.light}
          size="0.5em"
          data-for="main-tooltip"
          data-tip={workOrder.status}
        />
      );
  }
};
