import {
  IReportContentSettingsModel,
  IReportHeadlineSettingsModel,
  IReportInstanceSettingsModel,
  IReportSectionsSettingsModel,
} from '.';

export interface IReportSettingsModel {
  headline: IReportHeadlineSettingsModel;
  content: IReportContentSettingsModel;
  sections: IReportSectionsSettingsModel;
  instance: IReportInstanceSettingsModel;
  viewOnWebOnly: boolean;
}
