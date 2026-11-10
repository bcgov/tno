import { OptionItem, type IActionModel } from 'tno-core';

export const getActionOptions = (actions: IActionModel[]) => {
  return actions.map((a) => new OptionItem(a.name, a.id, !a.isEnabled));
};
