import {
  IReportInstanceModel,
  IReportScheduleModel,
  IReportSectionModel,
  IReportSettingsModel,
  IReportTemplateModel,
  ISortableModel,
  IUserModel,
} from '.';

export interface IReportModel extends ISortableModel<number> {
  ownerId?: number;
  owner?: IUserModel;
  templateId: number;
  template?: IReportTemplateModel;
  isPublic: boolean;
  settings: IReportSettingsModel;
  sections: IReportSectionModel[];
  subscribers: IUserModel[];
  instances: IReportInstanceModel[];
  schedules: IReportScheduleModel[];
}
