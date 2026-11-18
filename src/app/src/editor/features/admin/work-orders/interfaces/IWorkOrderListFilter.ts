import { type ISortBy } from '@/editor/features/interfaces';
import { type WorkOrderStatusName, type WorkOrderTypeName } from '@/shared';

export interface IWorkOrderListFilter {
  status?: WorkOrderStatusName | '';
  workType?: WorkOrderTypeName | '';
  keyword: string;
  pageIndex: number;
  pageSize: number;
  sort: ISortBy[];
}
