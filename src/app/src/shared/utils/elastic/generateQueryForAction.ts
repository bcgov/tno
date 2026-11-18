import { IFilterActionSettingsModel } from '../../hooks';

export const generateQueryForAction = (action: IFilterActionSettingsModel) => {
  let matches: any[] = [
    {
      match: {
        'actions.id': action.id,
      },
    },
  ];

  if (!!action.valueType)
    matches.push({
      match: {
        'actions.valueType': action.valueType,
      },
    });

  if (!!action.value && action.value.includes('*'))
    matches.push({
      wildcard: {
        'actions.value': action.value,
      },
    });
  else
    matches.push({
      match: {
        'actions.value': action.value,
      },
    });

  return {
    nested: {
      path: 'actions',
      query: {
        bool: {
          must: matches,
        },
      },
    },
  };
};
