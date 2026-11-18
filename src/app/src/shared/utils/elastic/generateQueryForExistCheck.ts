/**
 * Generates an Elasticsearch query for checking if a field exists or not.
 * @returns An Elasticsearch query.
 */
export const generateQueryForExistCheck = (field: string) => {
  if (!field) return undefined;
  return {
    nested: {
      path: field,
      query: {
        exists: {
          field: field,
        },
      },
    },
  };
};
