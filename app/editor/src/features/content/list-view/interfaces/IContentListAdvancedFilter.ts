import { IOptionItem } from 'components';
import { LogicalOperator } from 'hooks';

export interface IContentListAdvancedFilter {
  fieldType: IOptionItem;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  startDate?: string | null;
  endDate?: string | null;
}
