# `content` index — query recipes

All examples are `query_body` payloads for `MMI Elasticsearch:search`. Trim `_source`
and bound `size`/dates to keep responses decodable.

## 1. Top stories for a day

```json
{
  "_source": ["id", "headline", "publishedOn", "otherSource", "source.name"],
  "query": {
    "bool": {
      "must": [
        {
          "nested": {
            "path": "actions",
            "query": {
              "bool": {
                "must": [
                  { "match": { "actions.name": "Top Story" } },
                  { "match": { "actions.value": "true" } }
                ]
              }
            }
          }
        },
        {
          "range": {
            "publishedOn": { "gte": "2026-06-05T00:00:00Z", "lte": "2026-06-06T23:59:59Z" }
          }
        }
      ]
    }
  },
  "size": 100,
  "sort": [{ "publishedOn": { "order": "desc" } }]
}
```

Swap `Top Story` for `Featured Story` or `Alert` for those flags.

## 2. Coverage of a ministry/tag in a window

```json
{
  "_source": ["id", "headline", "publishedOn", "source.name"],
  "query": {
    "bool": {
      "must": [
        { "nested": { "path": "tags", "query": { "term": { "tags.code": "HLTH" } } } },
        {
          "range": {
            "publishedOn": { "gte": "2026-06-01T00:00:00Z", "lte": "2026-06-06T23:59:59Z" }
          }
        }
      ]
    }
  },
  "size": 50,
  "sort": [{ "publishedOn": { "order": "desc" } }]
}
```

## 3. Negative-sentiment items about a topic

```json
{
  "_source": ["id", "headline", "publishedOn", "source.name"],
  "query": {
    "bool": {
      "must": [
        {
          "nested": { "path": "topics", "query": { "match": { "topics.name": "Overdose crisis" } } }
        },
        {
          "nested": {
            "path": "tonePools",
            "query": {
              "bool": {
                "must": [
                  { "term": { "tonePools.name": "Default" } },
                  { "range": { "tonePools.value": { "lt": 0, "gt": -99 } } }
                ]
              }
            }
          }
        }
      ]
    }
  },
  "size": 50
}
```

## 4. Sentiment breakdown (counts by score)

```json
{
  "size": 0,
  "query": { "nested": { "path": "tags", "query": { "term": { "tags.code": "PSSG" } } } },
  "aggs": {
    "tone": {
      "nested": { "path": "tonePools" },
      "aggs": { "scores": { "terms": { "field": "tonePools.value", "size": 20 } } }
    }
  }
}
```

(Filter out `-99` when reporting; treat `>0` positive, `0` neutral, `<0` negative.)

## 5. Count only (no documents)

Set `"size": 0` and read `Total results`. Add filters as needed. Cheapest way to answer
"how many…".

## 6. Breakdown by source / media type

```json
{
  "size": 0,
  "query": {
    "range": { "publishedOn": { "gte": "2026-06-01T00:00:00Z", "lte": "2026-06-06T23:59:59Z" } }
  },
  "aggs": {
    "by_source": { "terms": { "field": "source.name.keyword", "size": 25 } },
    "by_media": { "terms": { "field": "mediaType.name.keyword", "size": 15 } }
  }
}
```

## 7. Keyword search across headline/summary/body

```json
{
  "_source": ["id", "headline", "publishedOn", "source.name"],
  "query": {
    "bool": {
      "must": [
        {
          "multi_match": {
            "query": "wildfire evacuation",
            "fields": ["headline^2", "summary", "body"]
          }
        }
      ]
    }
  },
  "size": 50,
  "sort": [{ "_score": { "order": "desc" } }]
}
```

Add a `publishedOn` range and/or a nested `tags`/`topics` filter to narrow.

## 8. Published-only view

Add `{"term": {"status": "Published"}}` to the `bool.must`. Drafts dominate the index,
so this matters for "what was published" questions.

## 9. ES|QL for tabular aggregate reports

```
FROM content
| WHERE publishedOn >= "2026-06-01T00:00:00Z"
| STATS count = COUNT(*) BY contentType
| SORT count DESC
```

Note: ES|QL handling of nested fields is limited — prefer DSL nested aggs (recipes 4 & 6)
for `actions`/`tags`/`topics`/`tonePools`.

## Debugging checklist (when you get 0 hits or a 400)

- Querying `actions`/`tags`/`topics`/`tonePools` without a `nested` wrapper → 0 hits. Wrap it.
- Matching `actions.value` with `term`/`.keyword` → 0 hits. Use `{"match": {"actions.value": "true"}}`.
- Aggregation throws 400 → toggle `.keyword` on the field (e.g. `source.name.keyword`, `mediaType.name.keyword`) — but `contentType`, `mediaTypeId`, `tags.code`, `tags.name`, `topics.name`, `actions.name`, `tonePools.name`, `tonePools.value`, `status` aggregate WITHOUT `.keyword`.
- `error decoding response body` → response too large. Trim `_source`, lower `size`, or use `size:0` aggs.
- "today" returns nothing with `now/d` → use an explicit UTC calendar range (BC items stamp 07:00:00Z; AV spills past UTC midnight).
- Source exact-match fails → leading spaces in some `source.name` values; use `match` not `term`.
