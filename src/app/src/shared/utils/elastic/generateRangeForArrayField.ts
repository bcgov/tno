/**
 * Generate an Elasticsearch query statement that will search for a range of values within an array field.
 * @param field Path to field.
 * @param values Value or values to search for.
 * @returns An elasticsearch query statement.
 */
export const generateRangeForArrayField = (
  field: string,
  values: any | any[],
  relation: 'intersects' | 'contains' | 'within' = 'intersects',
) => {
  if (values === undefined || values === null) return undefined;
  if (Array.isArray(values)) {
    const range = values as any[];
    return range.length
      ? {
          nested: {
            path: field.split('.')[0],
            query: {
              bool: {
                must: [
                  {
                    range: {
                      [field]: {
                        gte: range[0],
                        lte: range.length > 1 ? range[1] : range[0],
                        relation,
                      },
                    },
                  },
                ],
              },
            },
          },
        }
      : undefined;
  } else {
    return {
      nested: {
        path: field.split('.')[0],
        query: {
          bool: {
            must: [
              {
                range: {
                  [field]: {
                    gte: values,
                    lte: values,
                    relation,
                  },
                },
              },
            ],
          },
        },
      },
    };
  }
};
