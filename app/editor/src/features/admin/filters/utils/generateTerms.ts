/**
 * Generates an Elasticsearch query for a field that has one of the specified values.
 * @param field Field path.
 * @param values An array of values to search for.
 * @returns An Elasticsearch query.
 */
export const generateTerms = (field: string, values?: any[]) => {
  if (values === undefined || values === null) return undefined;
  return values.length > 0
    ? {
        terms: { [field]: values },
      }
    : undefined;
};
