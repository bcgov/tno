/**
 * Generates an Elasticsearch query for either a multi_match or a match depending on the specified values, this method supports wildcards.
 * @param fields Field path.
 * @param value The value(s) to search for.
 * @returns An Elasticsearch query for either a multi_match or a match.
 */
export const generateQueryString = (
  fields: string | string[],
  values: any,
  defaultSearchOperator?: string,
) => {
  if (values === undefined || values === null) return undefined;
  return {
    query_string: {
      query: values,
      fields,
      default_operator: defaultSearchOperator ?? 'and',
    },
  };
};
