import { AVOverviewItemTypeName } from '..';
import { IAuditColumnsModel } from '.';

export interface IAVOverviewSectionItemModel extends IAuditColumnsModel {
  id: number;
  sectionId: number;
  itemType: AVOverviewItemTypeName;
  time: string;
  summary: string;
  contentId?: number;
  sortOrder: number;
}
