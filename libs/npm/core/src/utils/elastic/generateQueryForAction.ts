export const generateQueryForAction = (
  id: number,
  value: string,
  valueType: string | undefined = 'Boolean',
) => {
  let matches: any[] = [
    {
      match: {
        'actions.id': id,
      },
    },
    {
      match: {
        'actions.valueType': valueType,
      },
    },
  ];
  if (!!value)
    matches = [
      ...matches,
      value === '*'
        ? {
            wildcard: {
              'actions.valueLabel': value,
            },
          }
        : {
            match: {
              'actions.valueLabel': value,
            },
          },
    ];
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
