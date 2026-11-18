/**
 * Generates an array of objects, each containing a 'match' query for the specified field.
 * @param fields Field path.
 * @param value The value(s) to search for.
 * @returns An Elasticsearch query.
 */
export const generateMatch = (field: string, values: any) => {
  if (values === undefined || values === null) return [];
  return values.map((value: string) => ({
    match: {
      [field]: value,
    },
  }));
};
