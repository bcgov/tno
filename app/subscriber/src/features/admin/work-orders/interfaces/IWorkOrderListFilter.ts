import { ISortBy } from 'features/content/list-view/interfaces';
import { WorkOrderStatusName, WorkOrderTypeName } from 'hooks';

export interface IWorkOrderListFilter {
  status: WorkOrderStatusName | '';
  workType: WorkOrderTypeName | '';
  keyword: string;
  pageIndex: number;
  pageSize: number;
  sort: ISortBy[];
}
