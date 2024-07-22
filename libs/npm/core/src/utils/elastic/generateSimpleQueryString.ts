export type SimpleQueryStringOptions = {
  default_operator?: 'and' | 'or';
  analyze_wildcard?: boolean;
  analyzer?: string;
  auto_generate_synonyms_phrase_query?: boolean;
  flags?: string;
  fuzzy_max_expansions?: number;
  fuzzy_prefix_length?: number;
  fuzzy_transpositions?: boolean;
  lenient?: boolean;
  minimum_should_match?: string;
  quote_field_suffix?: string;
};

const defaultSimpleQueryStringOptions: SimpleQueryStringOptions = {
  default_operator: 'and',
  auto_generate_synonyms_phrase_query: true,
  analyze_wildcard: true,
  fuzzy_max_expansions: 50,
  fuzzy_prefix_length: 0,
  fuzzy_transpositions: true,
};

/**
 * Generates an Elasticsearch query for either a multi_match or a match depending on the specified values.
 * @param fields Array of fields you wish to search.
 * @param query Query string you wish to parse and use for search.
 * @param options Search options.
 * @returns An Elasticsearch query for either a multi_match or a match.
 */
export const generateSimpleQueryString = (
  fields: string | string[],
  query: string,
  options: SimpleQueryStringOptions,
) => {
  const config = { ...defaultSimpleQueryStringOptions, ...options };

  if (query === undefined || query === null) return undefined;
  return {
    simple_query_string: {
      ...config,
      query,
      fields,
    },
  };
};
