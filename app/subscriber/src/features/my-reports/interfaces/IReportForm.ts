import { IReportModel } from 'tno-core';

import { IReportSectionForm } from './IReportSectionForm';

export interface IReportForm extends Omit<IReportModel, 'sections'> {
  hideEmptySections: boolean;
  sections: IReportSectionForm[];
}
