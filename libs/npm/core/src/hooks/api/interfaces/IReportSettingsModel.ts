import {
  IReportContentSettingsModel,
  IReportHeadlineSettingsModel,
  IReportSectionsSettingsModel,
  IReportSubjectSettingsModel,
} from '.';

export interface IReportSettingsModel {
  viewOnWebOnly: boolean;
  subject: IReportSubjectSettingsModel;
  headline: IReportHeadlineSettingsModel;
  content: IReportContentSettingsModel;
  sections: IReportSectionsSettingsModel;
}
