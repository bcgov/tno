import { CategoryTypeName } from '../constants';

export interface ICategoryFilter {
  name?: string;
  description?: string;
  categoryType?: CategoryTypeName;
}
