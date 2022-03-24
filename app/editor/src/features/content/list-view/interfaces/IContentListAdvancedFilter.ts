import { IOptionItem } from 'components/form';
import { LogicalOperator } from 'hooks/api-editor';

export interface IContentListAdvancedFilter {
  fieldType: IOptionItem;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  startDate?: string | null;
  endDate?: string | null;
}
