import { IAuditColumnsModel } from '.';
import { AVOverviewItemTypeName } from '..';

export interface IAVOverviewSectionItemModel extends IAuditColumnsModel {
  id: number;
  sectionId: number;
  itemType: AVOverviewItemTypeName;
  time: string;
  summary: string;
  contentId?: number;
  sortOrder: number;
}
