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
  weeklyPrint: ISourceModel[],
  cpWire: ISourceModel[],
  talkRadio: ISourceModel[],
  onlinePrint: ISourceModel[],
  newsRadio: ISourceModel[],
  television: ISourceModel[],
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
      options: weeklyPrint,
    },
    {
      key: 'dailyPrint',
      label: 'Daily Print',
      options: dailyPrint,
    },
    {
      key: 'talkRadio',
      label: 'Talk Radio',
      options: talkRadio,
    },
    {
      key: 'television',
      label: 'Television',
      options: television,
    },
    {
      key: 'online',
      label: 'Online',
      options: onlinePrint,
    },
    {
      key: 'cpNews',
      label: 'CP News',
      options: cpWire,
    },
    {
      key: 'newsRadio',
      label: 'News Radio',
      options: newsRadio,
    },
  ];
};
