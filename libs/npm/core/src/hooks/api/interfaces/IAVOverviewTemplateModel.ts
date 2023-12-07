import { AVOverviewTemplateTypeName } from '../constants';
import {
  IAuditColumnsModel,
  IAVOverviewTemplateSectionModel,
  IReportTemplateModel,
  IUserAVOverviewModel,
} from '.';

export interface IAVOverviewTemplateModel extends IAuditColumnsModel {
  templateType: AVOverviewTemplateTypeName;
  templateId: number;
  template?: IReportTemplateModel;
  sections: IAVOverviewTemplateSectionModel[];
  subscribers: IUserAVOverviewModel[];
}
