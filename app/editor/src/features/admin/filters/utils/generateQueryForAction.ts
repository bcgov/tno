export const generateQueryForAction = (
  id: number,
  value: string,
  valueType: string | undefined = 'Boolean',
) => {
  return {
    nested: {
      path: 'actions',
      query: {
        bool: {
          must: [
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
            value === '*'
              ? {
                  wildcard: {
                    'actions.value': value,
                  },
                }
              : {
                  match: {
                    'actions.value': value,
                  },
                },
          ],
        },
      },
    },
  };
};
