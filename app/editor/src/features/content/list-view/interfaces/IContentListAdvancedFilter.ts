import { LogicalOperator } from 'tno-core';

import { advancedSearchKeys } from '../constants';

export interface IContentListAdvancedFilter {
  fieldType: advancedSearchKeys;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  startDate?: string | null;
  endDate?: string | null;
}
