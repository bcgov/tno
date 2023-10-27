import { LogicalOperator } from 'tno-core';

import { AdvancedSearchKeys } from '../constants';

export interface IContentListAdvancedFilter {
  fieldType: AdvancedSearchKeys;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  startDate?: string | null;
  endDate?: string | null;
}
