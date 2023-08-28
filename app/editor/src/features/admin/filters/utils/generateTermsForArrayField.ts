/**
 * Generates an Elasticsearch query for an array field that has one of the specified values.
 * @param field Field path.
 * @param values An array of values to search for.
 * @returns An Elasticsearch query.
 */
export const generateTermsForArrayField = (field: string, values?: any[]) => {
  if (values === undefined || values === null || (Array.isArray(values) && values.length === 0))
    return undefined;
  return Array.isArray(values)
    ? {
        nested: {
          path: field.split('.')[0],
          query: {
            terms: { [field]: values },
          },
        },
      }
    : {
        nested: {
          path: field.split('.')[0],
          query: {
            term: { [field]: values },
          },
        },
      };
};
