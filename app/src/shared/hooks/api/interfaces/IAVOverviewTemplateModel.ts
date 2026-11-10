import {
  IAuditColumnsModel,
  IAVOverviewTemplateSectionModel,
  IReportTemplateModel,
  IUserAVOverviewModel,
} from '.';
import { AVOverviewTemplateTypeName } from '../constants';

export interface IAVOverviewTemplateModel extends IAuditColumnsModel {
  templateType: AVOverviewTemplateTypeName;
  templateId: number;
  template?: IReportTemplateModel;
  sections: IAVOverviewTemplateSectionModel[];
  subscribers: IUserAVOverviewModel[];
}
