import { ListOptionName } from 'tno-core';

export interface IGroupOption {
  id: number;
  name: string;
  listOption: ListOptionName;
  sortOrder: number;
  selected: boolean;
}
