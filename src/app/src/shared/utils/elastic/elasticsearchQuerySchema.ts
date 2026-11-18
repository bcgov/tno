/**
 * A JSON schema for the Elasticsearch search request body, written for a JSON editor: it drives
 * completion, hover documentation and validation of the queries the filters run. It covers the
 * search body and query DSL the application uses, and allows the rarer query types without
 * describing them.
 */

const clause = { $ref: '#/definitions/queryClause' };
const clauses = {
  description: 'One clause, or a list of clauses.',
  oneOf: [clause, { type: 'array', items: clause }],
};
const scalar = { type: ['string', 'number', 'boolean'] };
const operator = {
  description: 'How the terms of the text combine: any term (or) or every term (and).',
  enum: ['and', 'or', 'AND', 'OR'],
};
const minimumShouldMatch = {
  description:
    'How many optional clauses or terms must match, as a count (2), a percentage ("75%"), or a combination ("2<75%").',
  type: ['integer', 'string'],
};
const boost = { description: 'Multiplies the relevance score of the clause.', type: 'number' };
const name = {
  description: 'Names the clause; matched_queries then reports which named clauses matched.',
  type: 'string',
};
const fields = {
  description: 'The fields to search; a field can be boosted with ^, i.e. "headline^2".',
  type: 'array',
  items: { type: 'string' },
};
const fuzziness = {
  description: 'The edit distance allowed per term: AUTO, 0, 1 or 2.',
  type: ['string', 'integer'],
};
const textQueryOptions = {
  type: 'object',
  properties: {
    query: { description: 'The text to search for.', type: 'string' },
    operator,
    minimum_should_match: minimumShouldMatch,
    fuzziness,
    analyzer: { description: 'The analyzer that turns the text into terms.', type: 'string' },
    boost,
    zero_terms_query: {
      description: 'What matches when the analyzer removes every term.',
      enum: ['none', 'all'],
    },
    slop: {
      description: 'How far apart the terms of a phrase may be, in positions.',
      type: 'integer',
    },
    max_expansions: {
      description: 'How many terms a prefix or fuzzy term may expand to.',
      type: 'integer',
    },
    _name: name,
  },
  required: ['query'],
};
const perFieldText = {
  description: 'The field to search, with the text or an options object.',
  type: 'object',
  minProperties: 1,
  maxProperties: 1,
  additionalProperties: { oneOf: [scalar, textQueryOptions] },
};
const anyObject = (description: string) => ({ description, type: 'object' });

/** A JSON schema for the Elasticsearch search request body, for editor completion and validation. */
export const elasticsearchQuerySchema = {
  $schema: 'http://json-schema.org/draft-07/schema#',
  title: 'Elasticsearch search request',
  description: 'The request body sent to Elasticsearch to find stories.',
  type: 'object',
  properties: {
    query: { ...clause, description: 'The query that selects the stories.' },
    size: {
      description: 'How many stories to return (default 10, at most 10,000).',
      type: 'integer',
      minimum: 0,
      maximum: 10000,
    },
    from: { description: 'How many stories to skip, for paging.', type: 'integer', minimum: 0 },
    sort: {
      description: 'The sort order: a field, a list of fields, or field: order pairs.',
      oneOf: [
        { type: 'string' },
        { $ref: '#/definitions/sortItem' },
        { type: 'array', items: { $ref: '#/definitions/sortItem' } },
      ],
    },
    _source: {
      description:
        'Which stored fields each story returns: true, false, a field list, or includes/excludes.',
      oneOf: [
        { type: 'boolean' },
        { type: 'string' },
        { type: 'array', items: { type: 'string' } },
        {
          type: 'object',
          properties: {
            includes: { type: 'array', items: { type: 'string' } },
            excludes: { type: 'array', items: { type: 'string' } },
          },
          additionalProperties: false,
        },
      ],
    },
    collapse: {
      description:
        'Returns one story per distinct value of a keyword, number, date or boolean field.',
      type: 'object',
      properties: {
        field: { description: 'The field to collapse on.', type: 'string' },
        inner_hits: anyObject('Also returns the other stories of each group.'),
        max_concurrent_group_searches: { type: 'integer' },
      },
      required: ['field'],
      additionalProperties: false,
    },
    aggs: { $ref: '#/definitions/aggregations' },
    aggregations: { $ref: '#/definitions/aggregations' },
    track_total_hits: {
      description:
        'Whether to count every match (true), up to a limit (number), or not at all (false).',
      type: ['boolean', 'integer'],
    },
    timeout: { description: 'How long the search may run, i.e. "10s".', type: 'string' },
    min_score: { description: 'Leaves out stories scoring below this.', type: 'number' },
    post_filter: { ...clause, description: 'Filters the stories after aggregations are computed.' },
    highlight: anyObject('Returns the matching text fragments of each story.'),
    runtime_mappings: {
      description: 'Fields computed at search time from a script.',
      type: 'object',
      additionalProperties: {
        type: 'object',
        properties: {
          type: {
            enum: [
              'boolean',
              'composite',
              'date',
              'double',
              'geo_point',
              'ip',
              'keyword',
              'long',
              'lookup',
            ],
          },
          script: {
            oneOf: [
              { type: 'string' },
              {
                type: 'object',
                properties: { source: { type: 'string' }, params: { type: 'object' } },
              },
            ],
          },
          format: { type: 'string' },
        },
        required: ['type'],
      },
    },
    search_after: {
      description: 'The sort values of the last story of the previous page.',
      type: 'array',
    },
    fields: {
      description: 'Fields to return in the fields section, with formatting.',
      type: 'array',
    },
    docvalue_fields: { type: 'array' },
    stored_fields: { oneOf: [{ type: 'string' }, { type: 'array', items: { type: 'string' } }] },
    script_fields: anyObject('Fields computed from a script for each story.'),
    explain: { description: 'Returns how each score was computed.', type: 'boolean' },
    version: { description: 'Returns the document version of each story.', type: 'boolean' },
    seq_no_primary_term: { type: 'boolean' },
    terminate_after: { type: 'integer' },
    indices_boost: { type: 'array' },
    pit: anyObject('A point in time to search.'),
    knn: anyObject('A k-nearest-neighbour vector search.'),
    rescore: anyObject('Re-scores the top stories with a second query.'),
    suggest: anyObject('Suggestions for similar terms.'),
    profile: { type: 'boolean' },
    stats: { type: 'array', items: { type: 'string' } },
  },
  additionalProperties: false,
  definitions: {
    queryClause: {
      description: 'A query clause: exactly one query type.',
      type: 'object',
      minProperties: 1,
      maxProperties: 1,
      properties: {
        bool: {
          description:
            'Combines clauses: must and filter all match, should adds optional matches, must_not excludes.',
          type: 'object',
          properties: {
            must: {
              ...clauses,
              description: 'Every clause must match, and contributes to the score.',
            },
            filter: {
              ...clauses,
              description: 'Every clause must match, without affecting the score.',
            },
            should: {
              ...clauses,
              description:
                'Optional clauses; at least minimum_should_match of them must match when there is no must or filter.',
            },
            must_not: { ...clauses, description: 'No clause may match.' },
            minimum_should_match: minimumShouldMatch,
            boost,
            _name: name,
          },
          additionalProperties: false,
        },
        match: {
          ...perFieldText,
          description: 'Full-text search of one field; any term matches unless operator is "and".',
        },
        match_phrase: {
          ...perFieldText,
          description: 'The terms must appear in order, as a phrase.',
        },
        match_phrase_prefix: {
          ...perFieldText,
          description: 'A phrase whose last term is a prefix, for search-as-you-type.',
        },
        match_bool_prefix: {
          ...perFieldText,
          description: 'Each term matches, and the last term as a prefix.',
        },
        multi_match: {
          description: 'Full-text search across several fields.',
          type: 'object',
          properties: {
            query: { description: 'The text to search for.', type: 'string' },
            fields,
            type: {
              description:
                'How the fields combine: best_fields (the best field scores), most_fields (fields add up), cross_fields (terms may be in different fields), phrase, phrase_prefix, bool_prefix.',
              enum: [
                'best_fields',
                'most_fields',
                'cross_fields',
                'phrase',
                'phrase_prefix',
                'bool_prefix',
              ],
            },
            operator,
            minimum_should_match: minimumShouldMatch,
            fuzziness,
            slop: { type: 'integer' },
            tie_breaker: { type: 'number' },
            analyzer: { type: 'string' },
            boost,
            _name: name,
          },
          required: ['query'],
          additionalProperties: false,
        },
        query_string: {
          description:
            'Lucene query syntax: AND, OR, NOT, "phrases", wild*cards, field:value, (groups). Invalid syntax is an error.',
          type: 'object',
          properties: {
            query: { description: 'The query in Lucene syntax.', type: 'string' },
            fields,
            default_field: {
              description: 'The field searched when the query names none.',
              type: 'string',
            },
            default_operator: operator,
            analyze_wildcard: { type: 'boolean' },
            allow_leading_wildcard: { type: 'boolean' },
            minimum_should_match: minimumShouldMatch,
            lenient: {
              description: 'Ignores format errors such as text in a number field.',
              type: 'boolean',
            },
            fuzziness,
            phrase_slop: { type: 'integer' },
            quote_field_suffix: { type: 'string' },
            analyzer: { type: 'string' },
            time_zone: { type: 'string' },
            boost,
            _name: name,
          },
          required: ['query'],
          additionalProperties: false,
        },
        simple_query_string: {
          description:
            'A forgiving query syntax: + (and), | (or), - (not), "phrases", prefix*, (groups). Invalid syntax is ignored.',
          type: 'object',
          properties: {
            query: { description: 'The query in simple syntax.', type: 'string' },
            fields,
            default_operator: operator,
            flags: {
              description: 'The syntax features enabled, i.e. "AND|OR|NOT|PHRASE|PREFIX".',
              type: 'string',
            },
            analyze_wildcard: { type: 'boolean' },
            minimum_should_match: minimumShouldMatch,
            lenient: { type: 'boolean' },
            fuzzy_max_expansions: { type: 'integer' },
            fuzzy_prefix_length: { type: 'integer' },
            analyzer: { type: 'string' },
            quote_field_suffix: { type: 'string' },
            boost,
            _name: name,
          },
          required: ['query'],
          additionalProperties: false,
        },
        term: {
          description:
            'An exact value of a keyword, number, date or boolean field (not analyzed text).',
          type: 'object',
          minProperties: 1,
          maxProperties: 1,
          additionalProperties: {
            oneOf: [
              scalar,
              {
                type: 'object',
                properties: {
                  value: scalar,
                  boost,
                  case_insensitive: { type: 'boolean' },
                  _name: name,
                },
                required: ['value'],
                additionalProperties: false,
              },
            ],
          },
        },
        terms: {
          description: 'Any of several exact values of a field.',
          type: 'object',
          minProperties: 1,
          maxProperties: 2,
          properties: { boost, _name: name },
          additionalProperties: {
            oneOf: [
              { type: 'array', items: scalar },
              {
                description: 'Looks the values up in another document.',
                type: 'object',
                properties: {
                  index: { type: 'string' },
                  id: { type: 'string' },
                  path: { type: 'string' },
                  routing: { type: 'string' },
                },
                required: ['index', 'id', 'path'],
              },
            ],
          },
        },
        terms_set: anyObject('Matches when enough of the given values match the field.'),
        range: {
          description: 'Values between bounds; dates accept math such as "now-7d/d".',
          type: 'object',
          minProperties: 1,
          maxProperties: 1,
          additionalProperties: {
            type: 'object',
            properties: {
              gt: { ...scalar, description: 'Greater than.' },
              gte: { ...scalar, description: 'Greater than or equal to.' },
              lt: { ...scalar, description: 'Less than.' },
              lte: { ...scalar, description: 'Less than or equal to.' },
              format: { description: 'The date format of the bounds.', type: 'string' },
              time_zone: {
                description: 'The time zone of date bounds, i.e. "America/Vancouver".',
                type: 'string',
              },
              relation: { enum: ['INTERSECTS', 'CONTAINS', 'WITHIN'] },
              boost,
              _name: name,
            },
            additionalProperties: false,
          },
        },
        exists: {
          description: 'Stories that have any value in the field.',
          type: 'object',
          properties: { field: { type: 'string' }, boost, _name: name },
          required: ['field'],
          additionalProperties: false,
        },
        prefix: {
          description: 'Values of a keyword field that start with the text.',
          type: 'object',
          minProperties: 1,
          maxProperties: 1,
          additionalProperties: {
            oneOf: [
              { type: 'string' },
              {
                type: 'object',
                properties: {
                  value: { type: 'string' },
                  boost,
                  case_insensitive: { type: 'boolean' },
                  _name: name,
                },
                required: ['value'],
              },
            ],
          },
        },
        wildcard: {
          description: 'Values of a keyword field matching a pattern with * and ?.',
          type: 'object',
          minProperties: 1,
          maxProperties: 1,
          additionalProperties: {
            oneOf: [
              { type: 'string' },
              {
                type: 'object',
                properties: {
                  value: { type: 'string' },
                  boost,
                  case_insensitive: { type: 'boolean' },
                  rewrite: { type: 'string' },
                  _name: name,
                },
                required: ['value'],
              },
            ],
          },
        },
        regexp: {
          description: 'Values of a keyword field matching a regular expression.',
          type: 'object',
          minProperties: 1,
          maxProperties: 1,
          additionalProperties: {
            oneOf: [
              { type: 'string' },
              {
                type: 'object',
                properties: {
                  value: { type: 'string' },
                  flags: { type: 'string' },
                  case_insensitive: { type: 'boolean' },
                  max_determinized_states: { type: 'integer' },
                  boost,
                  _name: name,
                },
                required: ['value'],
              },
            ],
          },
        },
        fuzzy: anyObject('Values within an edit distance of the text.'),
        ids: {
          description: 'Stories with the given document ids.',
          type: 'object',
          properties: {
            values: { type: 'array', items: { type: ['string', 'integer'] } },
            boost,
            _name: name,
          },
          required: ['values'],
          additionalProperties: false,
        },
        nested: {
          description:
            'Queries a nested object field; every condition in the query must match the same nested object.',
          type: 'object',
          properties: {
            path: { description: 'The nested field, i.e. "actions" or "tags".', type: 'string' },
            query: { ...clause, description: 'The query run against each nested object.' },
            score_mode: {
              description: 'How the scores of the matching nested objects combine.',
              enum: ['avg', 'max', 'min', 'none', 'sum'],
            },
            ignore_unmapped: { type: 'boolean' },
            inner_hits: anyObject('Also returns the matching nested objects.'),
            boost,
            _name: name,
          },
          required: ['path', 'query'],
          additionalProperties: false,
        },
        match_all: {
          description: 'Every story.',
          type: 'object',
          properties: { boost, _name: name },
          additionalProperties: false,
        },
        match_none: { description: 'No story.', type: 'object', additionalProperties: false },
        constant_score: {
          description: 'Runs a filter and gives every match the same score.',
          type: 'object',
          properties: { filter: clause, boost, _name: name },
          required: ['filter'],
          additionalProperties: false,
        },
        dis_max: {
          description: 'The best-scoring of several queries.',
          type: 'object',
          properties: {
            queries: { type: 'array', items: clause },
            tie_breaker: { type: 'number' },
            boost,
            _name: name,
          },
          required: ['queries'],
          additionalProperties: false,
        },
        boosting: {
          description: 'Lowers the score of stories matching the negative query.',
          type: 'object',
          properties: {
            positive: clause,
            negative: clause,
            negative_boost: { type: 'number' },
            _name: name,
          },
          required: ['positive', 'negative', 'negative_boost'],
          additionalProperties: false,
        },
        function_score: {
          description: 'Changes scores with functions such as field values, decay or random.',
          type: 'object',
          properties: {
            query: clause,
            functions: { type: 'array', items: { type: 'object' } },
            boost_mode: { enum: ['multiply', 'replace', 'sum', 'avg', 'max', 'min'] },
            score_mode: { enum: ['multiply', 'sum', 'avg', 'first', 'max', 'min'] },
            max_boost: { type: 'number' },
            min_score: { type: 'number' },
            boost,
            _name: name,
          },
        },
        script: anyObject('A script that decides whether each story matches.'),
        script_score: anyObject('A script that computes the score of each story.'),
        more_like_this: anyObject('Stories similar to the given text or documents.'),
        combined_fields: anyObject('Full-text search treating several fields as one.'),
        intervals: anyObject('Terms by their order and proximity.'),
        percolate: anyObject('Stored queries that match a document.'),
        pinned: anyObject('Promotes given documents to the top.'),
        rank_feature: anyObject('Boosts by a rank_feature field.'),
        distance_feature: anyObject('Boosts by closeness to a date or point.'),
        has_child: anyObject('Parents of matching child documents.'),
        has_parent: anyObject('Children of matching parent documents.'),
        parent_id: anyObject('Children of a parent document.'),
        geo_bounding_box: anyObject('Points within a box.'),
        geo_distance: anyObject('Points within a distance.'),
        geo_polygon: anyObject('Points within a polygon.'),
        geo_shape: anyObject('Shapes related to a shape.'),
        shape: anyObject('Shapes related to a shape.'),
        span_term: anyObject('A span query.'),
        span_near: anyObject('A span query.'),
        span_or: anyObject('A span query.'),
        span_not: anyObject('A span query.'),
        span_first: anyObject('A span query.'),
        span_multi: anyObject('A span query.'),
        span_containing: anyObject('A span query.'),
        span_within: anyObject('A span query.'),
        span_field_masking: anyObject('A span query.'),
        wrapper: anyObject('A query given as base64 JSON.'),
        knn: anyObject('A k-nearest-neighbour vector query.'),
        sparse_vector: anyObject('A sparse vector query.'),
        text_expansion: anyObject('A text expansion query.'),
        semantic: anyObject('A semantic text query.'),
        rule: anyObject('A query rules query.'),
      },
      additionalProperties: false,
    },
    sortItem: {
      oneOf: [
        { type: 'string' },
        {
          type: 'object',
          minProperties: 1,
          maxProperties: 1,
          additionalProperties: {
            oneOf: [
              { enum: ['asc', 'desc'] },
              {
                type: 'object',
                properties: {
                  order: { enum: ['asc', 'desc'] },
                  missing: {
                    description: 'Where stories without the field sort: _first, _last or a value.',
                    type: ['string', 'number'],
                  },
                  mode: { enum: ['min', 'max', 'sum', 'avg', 'median'] },
                  nested: anyObject('Sorts by a nested field.'),
                  unmapped_type: { type: 'string' },
                  format: { type: 'string' },
                  numeric_type: { enum: ['double', 'long', 'date', 'date_nanos'] },
                },
                additionalProperties: false,
              },
            ],
          },
        },
      ],
    },
    aggregations: {
      description: 'Named aggregations computed over the matching stories.',
      type: 'object',
      additionalProperties: { $ref: '#/definitions/aggregation' },
    },
    aggregation: {
      type: 'object',
      properties: {
        terms: {
          description: 'The most common values of a field.',
          type: 'object',
          properties: {
            field: { type: 'string' },
            size: { description: 'How many values to return.', type: 'integer' },
            order: { type: ['object', 'array'] },
            missing: scalar,
            min_doc_count: { type: 'integer' },
            include: { type: ['string', 'array'] },
            exclude: { type: ['string', 'array'] },
          },
        },
        date_histogram: {
          description: 'Counts per date interval.',
          type: 'object',
          properties: {
            field: { type: 'string' },
            calendar_interval: { description: 'i.e. "1d", "week", "month".', type: 'string' },
            fixed_interval: { description: 'i.e. "12h", "30m".', type: 'string' },
            format: { type: 'string' },
            time_zone: { type: 'string' },
            min_doc_count: { type: 'integer' },
            missing: { type: 'string' },
          },
        },
        histogram: anyObject('Counts per numeric interval.'),
        range: {
          description: 'Counts per value range.',
          type: 'object',
          properties: {
            field: { type: 'string' },
            ranges: {
              type: 'array',
              items: { type: 'object', properties: { from: {}, to: {}, key: { type: 'string' } } },
            },
          },
        },
        date_range: anyObject('Counts per date range.'),
        nested: {
          description:
            'Aggregates the objects of a nested field; its sub-aggregations use the nested fields.',
          type: 'object',
          properties: { path: { type: 'string' } },
          required: ['path'],
        },
        reverse_nested: anyObject('Returns from a nested aggregation to the parent stories.'),
        filter: { ...clause, description: 'The stories matching a query.' },
        filters: anyObject('Buckets for several queries.'),
        avg: { $ref: '#/definitions/metric' },
        min: { $ref: '#/definitions/metric' },
        max: { $ref: '#/definitions/metric' },
        sum: { $ref: '#/definitions/metric' },
        stats: { $ref: '#/definitions/metric' },
        extended_stats: { $ref: '#/definitions/metric' },
        value_count: { $ref: '#/definitions/metric' },
        cardinality: {
          description: 'The approximate number of distinct values.',
          type: 'object',
          properties: {
            field: { type: 'string' },
            precision_threshold: { type: 'integer' },
            missing: scalar,
          },
        },
        percentiles: anyObject('Percentiles of a numeric field.'),
        top_hits: {
          description: 'The top stories of each bucket.',
          type: 'object',
          properties: {
            size: { type: 'integer' },
            from: { type: 'integer' },
            sort: {},
            _source: {},
          },
        },
        significant_terms: anyObject('Values unusually common in the matches.'),
        sampler: anyObject('Aggregates a sample of the best matches.'),
        composite: anyObject('Pages through all the value combinations of several sources.'),
        multi_terms: anyObject('The most common value combinations of several fields.'),
        missing: anyObject('The stories without a value in the field.'),
        global: anyObject('Every story, ignoring the query.'),
        bucket_sort: anyObject('Sorts or pages the parent buckets.'),
        bucket_script: anyObject('A script over the parent buckets.'),
        aggs: { $ref: '#/definitions/aggregations' },
        aggregations: { $ref: '#/definitions/aggregations' },
        meta: { type: 'object' },
      },
      additionalProperties: { type: 'object' },
    },
    metric: {
      type: 'object',
      properties: {
        field: { type: 'string' },
        missing: scalar,
        script: {},
      },
    },
  },
};
