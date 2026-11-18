/**
 * Generates an Elasticsearch query for a field that has one of the specified vale.
 * @param field Field path.
 * @param values A value to search for.
 * @returns An Elasticsearch query.
 */
export const generateTerm = (field: string, value?: any) => {
  if (value === undefined || value === null) return undefined;
  // A NaN - what `Number(...)` returns for a value that is not a number - is serialized as JSON
  // null, and Elasticsearch rejects the whole search with 'field name is null or empty'. There is
  // no term to match, so there is no clause.
  if (typeof value === 'number' && !Number.isFinite(value)) return undefined;
  return value.toString().includes('*')
    ? {
        wildcard: { [field]: value },
      }
    : {
        term: { [field]: value },
      };
};
