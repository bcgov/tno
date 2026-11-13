import { WorkOrderStatusName, WorkOrderTypeName } from '@/shared';
import { ISortBy } from '@/subscriber/features/utils/interfaces';

export interface IWorkOrderListFilter {
  status: WorkOrderStatusName | '';
  workType: WorkOrderTypeName | '';
  keyword: string;
  pageIndex: number;
  pageSize: number;
  sort: ISortBy[];
}
