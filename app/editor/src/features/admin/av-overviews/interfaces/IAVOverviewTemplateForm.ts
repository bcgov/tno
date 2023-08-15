import { IAVOverviewTemplateModel } from 'tno-core';

export interface IAVOverviewTemplateForm extends IAVOverviewTemplateModel {
  // Internal property to identify a new record.
  // This is required because the primary key is an enum with a value of 0.
  isNew?: boolean;
}
