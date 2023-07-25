import { ILookupModel, ISourceModel } from 'tno-core';

import { ISubMediaGroupExpanded } from '../interfaces';

export interface ISubMediaGroupItem {
  label: string;
  options: any[];
  key: keyof ISubMediaGroupExpanded;
}
/** will contain lookup options as well as labels for corresponding
 * section
 */
export const SubMediaGroups = (
  dailyPapers: ISourceModel[],
  all: ISourceModel[],
  internet: ILookupModel[],
): ISubMediaGroupItem[] => {
  return [
    {
      key: 'all',
      label: 'All',
      options: all,
    },
    {
      key: 'regionalPapers',
      label: 'Regional Papers',
      options: [],
    },
    {
      key: 'dailyPapers',
      label: 'Daily Papers',
      options: dailyPapers,
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
      key: 'internet',
      label: 'Internet',
      options: [],
    },
    {
      key: 'cpNews',
      label: 'CP News',
      options: [],
    },
  ];
};
