import { type IContributorModel } from '@/shared';

export interface IContributorForm extends Omit<IContributorModel, 'sourceId'> {
  sourceId?: number | '';
}
