/**
 * Generate Elasticsearch tag search query
 *
 * @param searchText Original search text
 * @returns Elasticsearch nested query for tags or undefined if no search terms
 */
export const generateTagsQuery = (searchText: string) => {
  // Preprocess the search text to extract keywords for tag matching
  // Define regex for Elasticsearch special operators that need to be removed
  const elasticOperators = /[+|"*()~-]/g;

  // Normalize search text by replacing Elasticsearch operators with spaces
  const preprocessedSearch = searchText.replace(elasticOperators, ' ');

  // Extract individual search terms by splitting on whitespace and removing empty entries
  const rawSearchTerms = preprocessedSearch.split(/\s+/).filter(Boolean);

  if (rawSearchTerms.length === 0) {
    return undefined;
  }

  // Generate the Elasticsearch nested query for tags using term queries for exact matching
  return {
    nested: {
      path: 'tags', // Path to the nested tags array in the document
      query: {
        bool: {
          should: rawSearchTerms.map((term) => ({
            term: {
              'tags.code': term.toUpperCase(), // Using uppercase for standardized tag comparison
            },
          })),
          minimum_should_match: 1, // Match succeeds if at least one term matches any tag
        },
      },
    },
  };
};

/**
 * Combine field query with tag query using Elasticsearch bool query
 *
 * @param fieldsQuery Elasticsearch query for searching regular document fields
 * @param tagsQuery Elasticsearch query for searching within tags
 * @returns Combined Elasticsearch bool query with OR logic between queries
 */
export const combineFieldAndTagQueries = (
  fieldsQuery: any | undefined,
  tagsQuery: any | undefined,
) => {
  // If both are undefined, return undefined
  if (!fieldsQuery && !tagsQuery) return undefined;

  // If only one query exists, return that query directly
  if (!fieldsQuery) return tagsQuery;
  if (!tagsQuery) return fieldsQuery;

  // If both queries exist, combine them with a bool query
  return {
    bool: {
      should: [fieldsQuery, tagsQuery],
      minimum_should_match: 1, // At least one query needs to be matched
    },
  };
};
