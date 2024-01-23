import {
  ActionName,
  IActionModel,
  IFilterActionSettingsModel
} from 'tno-core';


export const getFilterActions = (actions: IActionModel[]) => {
  const result: { [actionName: string]: IFilterActionSettingsModel; } = {};
  let actionName: keyof typeof ActionName;
  for (actionName in ActionName) {
    const value = ActionName[actionName];
    const action = actions.find((a) => a.name === value);
    if (action) {
      switch (value) {
        case ActionName.Homepage:
        case ActionName.TopStory:
          result[value] = {
            id: action.id,
            value: String(true),
            valueType: action?.valueType
          };
          break;
        case ActionName.Commentary:
          result[value] = {
            id: action.id,
            value: '*',
            valueType: action.valueType,
          };
          break;
        default:
          break;
      }
    }
  }
  return result;
};
