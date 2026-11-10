import { IAuditColumnsModel } from '.';
import { AVOverviewItemTypeName } from '..';

export interface IAVOverviewTemplateSectionItemModel extends IAuditColumnsModel {
  id: number;
  sectionId: number;
  itemType: AVOverviewItemTypeName;
  time: string;
  summary: string;
  sortOrder: number;
}
