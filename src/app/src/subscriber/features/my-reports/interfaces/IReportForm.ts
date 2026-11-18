import { IReportModel, IReportSettingsModel } from '@/shared';

import { IReportSectionForm } from './IReportSectionForm';

export interface IReportForm extends Omit<IReportModel, 'sections' | 'settings'> {
  hideEmptySections: boolean;
  /**
   * Whether the current instance's stories have been loaded into the form. They are loaded only
   * when a page needs them; until then the instance's content is empty and must never be saved.
   */
  contentLoaded?: boolean;
  sections: IReportSectionForm[];
  // collectionOnly is declared here until a tno-core release includes it.
  settings: IReportSettingsModel & { collectionOnly?: boolean };
}
