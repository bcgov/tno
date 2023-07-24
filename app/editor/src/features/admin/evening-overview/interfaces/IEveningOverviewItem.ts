import { EveningOverviewItemTypeName } from '../constants';

export interface IEveningOverviewItem {
  itemType: EveningOverviewItemTypeName;
  time: string;
  sortOrder: number;
  summary: string;
  contentId?: string;
  id: number;
  avOverviewSectionId: number;
}
