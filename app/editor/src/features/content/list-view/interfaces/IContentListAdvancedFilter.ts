import { LogicalOperator } from 'hooks/api-editor';

export interface IContentListAdvancedFilter {
  fieldType: string;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  startDate?: string | null;
  endDate?: string | null;
}
