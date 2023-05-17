import { IContributorModel } from 'tno-core';

export interface IContributorForm extends Omit<IContributorModel, 'sourceId'> {
  sourceId?: number | '';
}
