import { ISortPageFilter } from './ISortPageFilter';

export interface ILLMFilter extends ISortPageFilter {
  name?: string;
  description?: string;
  isPublic?: boolean;
}
