import { ISourceModel } from 'tno-core';

import { ISubMediaGroupExpanded } from '../interfaces';

export interface ISubMediaGroupItem {
  label: string;
  options: ISourceModel[];
  key: keyof ISubMediaGroupExpanded;
}
/** will contain lookup options as well as labels for corresponding
 * section
 */
export const SubMediaGroups = (
  dailyPrint: ISourceModel[],
  all: ISourceModel[],
): ISubMediaGroupItem[] => {
  return [
    {
      key: 'all',
      label: 'All',
      options: all,
    },
    {
      key: 'weeklyPrint',
      label: 'Weekly Print',
      options: [],
    },
    {
      key: 'dailyPrint',
      label: 'Daily Print',
      options: dailyPrint,
    },
    {
      key: 'talkRadio',
      label: 'Talk Radio',
      options: [],
    },
    {
      key: 'television',
      label: 'Television',
      options: [],
    },
    {
      key: 'online',
      label: 'Online',
      options: [],
    },
    {
      key: 'cpNews',
      label: 'CP News',
      options: [],
    },
    {
      key: 'newsRadio',
      label: 'News Radio',
      options: [],
    },
  ];
};
