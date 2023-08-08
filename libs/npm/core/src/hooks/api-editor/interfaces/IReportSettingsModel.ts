import {
  IReportContentSettingsModel,
  IReportHeadlineSettingsModel,
  IReportInstanceSettingsModel,
  IReportSectionsSettingsModel,
  IReportSubjectSettingsModel,
} from '.';

export interface IReportSettingsModel {
  viewOnWebOnly: boolean;
  subject: IReportSubjectSettingsModel;
  headline: IReportHeadlineSettingsModel;
  content: IReportContentSettingsModel;
  sections: IReportSectionsSettingsModel;
  instance: IReportInstanceSettingsModel;
}
