/**
 * Generates an Elasticsearch query for a field that has one of the specified vale.
 * @param field Field path.
 * @param values A value to search for.
 * @returns An Elasticsearch query.
 */
export const generateTerm = (field: string, value?: any) => {
  if (value === undefined || value === null) return undefined;
  return value.toString().includes('*')
    ? {
        wildcard: { [field]: value },
      }
    : {
        term: { [field]: value },
      };
};
