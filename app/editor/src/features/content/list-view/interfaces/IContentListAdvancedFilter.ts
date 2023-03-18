import { LogicalOperator } from 'tno-core';

export interface IContentListAdvancedFilter {
  fieldType: string;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  startDate?: string | null;
  endDate?: string | null;
}
