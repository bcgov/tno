/**
 * Generates an Elasticsearch query for either a multi_match or a match depending on the specified values.
 * @param fields Field path.
 * @param value The value(s) to search for.
 * @returns An Elasticsearch query for either a multi_match or a match.
 */
export const generateSimpleQueryString = (
  fields: string | string[],
  values: any,
  defaultOperator?: string,
) => {
  if (values === undefined || values === null) return undefined;
  return {
    simple_query_string: {
      query: values,
      fields,
      default_operator: defaultOperator ?? 'and',
    },
  };
};
