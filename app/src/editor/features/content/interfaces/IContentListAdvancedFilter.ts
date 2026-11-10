import { type LogicalOperator } from '@/shared';

import { type AdvancedSearchKeys } from '../constants';

export interface IContentListAdvancedFilter {
  fieldType: AdvancedSearchKeys;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  startDate?: string | null;
  endDate?: string | null;
  secondaryFieldType?: AdvancedSearchKeys;
  secondaryLogicalOperator?: LogicalOperator | '';
  secondarySearchTerm?: string;
  secondaryStartDate?: string | null;
  secondaryEndDate?: string | null;
}
