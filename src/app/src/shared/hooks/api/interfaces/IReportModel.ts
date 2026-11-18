import {
  IReportInstanceModel,
  IReportScheduleModel,
  IReportSectionModel,
  IReportSettingsModel,
  IReportTemplateModel,
  ISortableModel,
  IUserModel,
  IUserReportModel,
} from '.';

export interface IReportModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  templateId: number;
  template?: IReportTemplateModel;
  isPublic: boolean;
  settings: IReportSettingsModel;
  sections: IReportSectionModel[];
  subscribers: IUserReportModel[];
  instances: IReportInstanceModel[];
  events: IReportScheduleModel[];
}
