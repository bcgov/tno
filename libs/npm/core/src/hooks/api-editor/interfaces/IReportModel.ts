import { ReportTypeName } from '..';
import { IReportInstanceModel, ISortableModel, IUserModel } from '.';

export interface IReportModel extends ISortableModel<number> {
  ownerId: number;
  owner?: IUserModel;
  reportType: ReportTypeName;
  filter: any;
  template: string;
  isPublic: boolean;
  settings: any;
  subscribers: IUserModel[];
  instances: IReportInstanceModel[];
}
