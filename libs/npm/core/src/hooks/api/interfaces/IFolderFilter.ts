import { ISortPageFilter } from './ISortPageFilter';

export interface IFolderFilter extends ISortPageFilter {
  name?: string;
  ownerId?: number;
}
