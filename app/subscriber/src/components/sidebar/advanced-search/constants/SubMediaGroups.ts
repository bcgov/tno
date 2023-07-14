import { IOptionItem } from 'tno-core';

export interface ISubMediaGroupItem {
  label: string;
  options: IOptionItem[];
}
/** will contain lookup options as well as labels for corresponding
 * section
 */
export const SubMediaGroups: ISubMediaGroupItem[] = [
  {
    label: 'Regional Papers',
    options: [],
  },
  {
    label: 'Daily Papers',
    options: [],
  },
  {
    label: 'Talk Radio',
    options: [],
  },
  {
    label: 'Television',
    options: [],
  },
  {
    label: 'Internet',
    options: [],
  },
  {
    label: 'CP News',
    options: [],
  },
];
