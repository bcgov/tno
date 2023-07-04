import { ReportTypeName } from '..';
import { IReportInstanceModel, IReportTemplateModel, ISortableModel, IUserModel } from '.';

export interface IReportModel extends ISortableModel<number> {
  ownerId: number;
  owner?: IUserModel;
  reportType: ReportTypeName;
  filter: any;
  templateId: number;
  template: IReportTemplateModel;
  isPublic: boolean;
  settings: any;
  subscribers: IUserModel[];
  instances: IReportInstanceModel[];
}
