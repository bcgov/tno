/**
 * Generates an Elasticsearch query for either a multi_match or a match depending on the specified values.
 * @param fields Field path.
 * @param value The value(s) to search for.
 * @returns An Elasticsearch query for either a multi_match or a match.
 */
export const generateMultiMatch = (fields: string | string[], values: any) => {
  if (Array.isArray(fields)) {
    return [
      {
        multi_match: {
          query: values,
          fields,
        },
      },
    ];
  } else {
    return [
      {
        match: {
          [fields]: values,
        },
      },
    ];
  }
};
