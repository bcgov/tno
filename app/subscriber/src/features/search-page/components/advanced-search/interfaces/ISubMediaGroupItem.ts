import { IGroupOption } from './IGroupOption';

export interface ISubMediaGroupItem {
  label: string;
  options: IGroupOption[];
  key: number;
  sortOrder: number;
}
