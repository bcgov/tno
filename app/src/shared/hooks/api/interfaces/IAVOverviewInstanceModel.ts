import { IAuditColumnsModel, IAVOverviewSectionModel } from '.';
import { AVOverviewTemplateTypeName } from '../constants';

export interface IAVOverviewInstanceModel extends IAuditColumnsModel {
  id: number;
  templateType: AVOverviewTemplateTypeName;
  publishedOn: Date | string;
  isPublished: boolean;
  sections: IAVOverviewSectionModel[];
  response: any;
}
