/**
 * Generates an array of objects, each containing a 'match' query for the specified field.
 * @param fields Field path.
 * @param value The value(s) to search for.
 * @returns An Elasticsearch query.
 */
export const generateMatchPhrase = (fields: string[], values: string[]): any[] => {
  const matches = [];

  for (const field of fields) {
    for (const value of values) {
      matches.push({
        match_phrase: {
          [field]: value,
        },
      });
    }
  }

  return matches;
};
