import {
  IReportContentSettingsModel,
  IReportHeadlineSettingsModel,
  IReportSectionsSettingsModel,
  IReportSubjectSettingsModel,
} from '.';

export interface IReportSettingsModel {
  from: string;
  subject: IReportSubjectSettingsModel;
  headline: IReportHeadlineSettingsModel;
  content: IReportContentSettingsModel;
  sections: IReportSectionsSettingsModel;
  doNotSendEmail: boolean;
  /** The report only collects content for other reports; it is never sent and previews headlines only. */
  collectionOnly?: boolean;
}
