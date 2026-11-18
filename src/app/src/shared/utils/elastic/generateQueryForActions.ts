import { IFilterActionSettingsModel } from '../../hooks';
import { generateQueryForAction } from './generateQueryForAction';

export const generateQueryForActions = (actions: IFilterActionSettingsModel[]) => {
  return actions.map((a) => generateQueryForAction(a));
};
