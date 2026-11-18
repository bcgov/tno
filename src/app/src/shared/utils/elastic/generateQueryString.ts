export type QueryStringOptions = {
  default_operator?: 'and' | 'or';
  auto_generate_synonyms_phrase_query?: boolean;
  allow_leading_wildcard?: boolean;
  analyze_wildcard?: boolean;
  analyzer?: string;
  boost?: number;
  enable_position_increments?: boolean;
  fuzziness?: string;
  fuzzy_max_expansions?: number;
  fuzzy_prefix_length?: number;
  fuzzy_transpositions?: boolean;
  lenient?: boolean;
  max_determinized_states?: number;
  minimum_should_match?: string;
  quote_analyzer?: string;
  phrase_slop?: number;
  quote_field_suffix?: string;
  rewrite?: string;
  time_zone?: string;
};

const defaultQueryStringOptions: QueryStringOptions = {
  default_operator: 'and',
  auto_generate_synonyms_phrase_query: true,
  allow_leading_wildcard: true,
  analyze_wildcard: true,
  enable_position_increments: true,
  fuzzy_max_expansions: 50,
  fuzzy_prefix_length: 0,
  fuzzy_transpositions: true,
  max_determinized_states: 1000,
  phrase_slop: 0,
};

/**
 * Generates an Elasticsearch query for either a multi_match or a match depending on the specified values, this method supports wildcards.
 * @param fields Array of fields you wish to search.
 * @param query Query string you wish to parse and use for search.
 * @param options Search options.
 * @returns An Elasticsearch query for either a multi_match or a match.
 */
export const generateQueryString = (
  fields: string | string[],
  query: string,
  options: QueryStringOptions,
) => {
  const config = { ...defaultQueryStringOptions, ...options };

  if (query === undefined || query === null) return undefined;
  return {
    query_string: {
      ...config,
      query,
      fields,
    },
  };
};
