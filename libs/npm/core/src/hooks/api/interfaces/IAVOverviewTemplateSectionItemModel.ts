import { AVOverviewItemTypeName } from '..';
import { IAuditColumnsModel } from '.';

export interface IAVOverviewTemplateSectionItemModel extends IAuditColumnsModel {
  id: number;
  sectionId: number;
  itemType: AVOverviewItemTypeName;
  time: string;
  summary: string;
  sortOrder: number;
}
