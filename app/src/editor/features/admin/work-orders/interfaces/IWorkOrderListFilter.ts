import { type WorkOrderStatusName, type WorkOrderTypeName } from 'tno-core';

import { type ISortBy } from '@/editor/features/interfaces';

export interface IWorkOrderListFilter {
  status?: WorkOrderStatusName | '';
  workType?: WorkOrderTypeName | '';
  keyword: string;
  pageIndex: number;
  pageSize: number;
  sort: ISortBy[];
}
