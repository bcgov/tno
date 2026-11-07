import { IReportModel, IReportSettingsModel } from 'tno-core';

import { IReportSectionForm } from './IReportSectionForm';

export interface IReportForm extends Omit<IReportModel, 'sections' | 'settings'> {
  hideEmptySections: boolean;
  sections: IReportSectionForm[];
  // collectionOnly is declared here until a tno-core release includes it.
  settings: IReportSettingsModel & { collectionOnly?: boolean };
}
