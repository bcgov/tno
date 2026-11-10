import { OptionItem, type IActionModel } from '@/shared';

export const getActionOptions = (actions: IActionModel[]) => {
  return actions.map((a) => new OptionItem(a.name, a.id, !a.isEnabled));
};
