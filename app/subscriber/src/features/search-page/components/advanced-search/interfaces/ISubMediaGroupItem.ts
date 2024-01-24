import { ISourceModel } from 'tno-core';

import { ISubMediaGroupExpanded } from './ISubMediaGroupExpanded';

export interface ISubMediaGroupItem {
  label: string;
  options: ISourceModel[];
  key: keyof ISubMediaGroupExpanded;
}
