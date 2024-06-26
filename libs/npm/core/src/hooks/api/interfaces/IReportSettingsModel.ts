import {
  IReportContentSettingsModel,
  IReportHeadlineSettingsModel,
  IReportSectionsSettingsModel,
  IReportSubjectSettingsModel,
} from '.';

export interface IReportSettingsModel {
  subject: IReportSubjectSettingsModel;
  headline: IReportHeadlineSettingsModel;
  content: IReportContentSettingsModel;
  sections: IReportSectionsSettingsModel;
  doNotSendEmail: boolean;
}
