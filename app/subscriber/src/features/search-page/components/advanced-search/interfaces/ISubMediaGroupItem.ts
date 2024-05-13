import { ListOptionName } from 'tno-core';

import { IGroupOption } from './IGroupOption';

export interface ISubMediaGroupItem {
  label: string;
  options: IGroupOption[];
  key: number;
  listOption: ListOptionName;
  sortOrder: number;
}
