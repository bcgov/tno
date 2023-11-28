import { AVOverviewTemplateTypeName } from '../constants';
import { IAuditColumnsModel, IAVOverviewSectionModel } from '.';

export interface IAVOverviewInstanceModel extends IAuditColumnsModel {
  id: number;
  templateType: AVOverviewTemplateTypeName;
  publishedOn: Date | string;
  isPublished: boolean;
  sections: IAVOverviewSectionModel[];
  response: any;
}
