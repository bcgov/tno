import { ListOptionName } from '@/shared';

export interface IGroupOption {
  id: number;
  name: string;
  listOption: ListOptionName;
  sortOrder: number;
  selected: boolean;
}
