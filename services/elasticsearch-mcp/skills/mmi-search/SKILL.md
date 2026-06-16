---
name: mmi-search
description: Search the MMI Elasticsearch `content` index, a BC government media-monitoring corpus of ~8.9M news clips (print, online, radio, TV, wire). Use this skill WHENEVER the user asks to find, list, count, or analyze media content, news stories, clips, or coverage in Elasticsearch, OR mentions any of these by name — top stories, featured stories, alerts, commentary, tone or sentiment, tags (ministries), topics, media types, or sources. Also use it for queries like "top stories today", "what coverage did X get", "negative sentiment about Y", "clips from the Vancouver Sun", or any filtering by ministry/tag, topic, date, source, or sentiment. This skill defines the schema and the correct (often non-obvious) query patterns; consult it before writing any query against the `content` index, because several fields require nested queries or `.keyword` subfields that fail silently if used wrong.
---

# Searching the `content` index

The `content` index (MMI Elasticsearch connector) holds BC government media-monitoring
records: individual news items from print, online, radio, TV, and wire sources, each
enriched with editorial metadata (actions, tags, topics, sentiment).

**Scale:** ~8.9M documents. Always use `size` limits and date filters; never run an
unbounded fetch.

## Tooling

Use the `MMI Elasticsearch:search` tool with a Query DSL `query_body`. Use `esql` only
for tabular aggregate reports. Get field types via `MMI Elasticsearch:get_mappings`
(note: the full mapping is large and may fail to decode — prefer the schema in
`references/schema.md`).

### Critical: keep responses small

The connector **fails to decode very large responses** (you'll see
`error decoding response body`). To avoid this:

- Always set `_source` to only the fields you need (or use the `fields` parameter).
- Keep `size` modest (≤100). Use aggregations with `"size": 0` for counts/breakdowns.
- Never return the full `_source` of many documents at once.

## Query gotchas (read before writing any query)

These are the mistakes that silently return **0 results** or throw 400 errors:

1. **`actions`, `tags`, `topics`, `tonePools` are `nested` objects.** A flat
   `{"match": {"actions.name": "Top Story"}}` returns **0 hits**. You MUST wrap them in
   a `nested` query, and to match a name+value pair on the _same_ element, put both
   conditions inside one nested `bool`. See "Actions" below.
2. **`actions.value` is `text`, not boolean.** Match it with `{"match": {"actions.value": "true"}}`.
   `term` on `actions.value.keyword` returns 0.
3. **Aggregations:** some fields aggregate on the analyzed field, others need
   `.keyword`. Confirmed aggregatable: `actions.name`, `tags.code`, `tags.name`,
   `topics.name`, `tonePools.name`, `tonePools.value`, `contentType`, `mediaTypeId`,
   `mediaType.name.keyword`, `source.name.keyword`, `status`, `licenseId`. If an agg
   throws 400, try toggling `.keyword`.
4. **Date boundaries are UTC.** `publishedOn` is a UTC timestamp; BC print items often
   carry a 07:00:00Z stamp and TV/radio can spill into the next UTC day. For "today"
   (local), use a calendar range spanning the local day in UTC rather than `now/d`.
   See "Date filtering" below.
5. **`source.name` sometimes has a leading space** (e.g. `" Victoria Times Colonist"`,
   `" Kelowna Courier"`). Prefer `match` over exact `term` for source name, or account
   for the space.

## Schema

See `references/schema.md` for the full field list and types. The fields you'll use most:

| Field            | Type      | Notes                                                                                           |
| ---------------- | --------- | ----------------------------------------------------------------------------------------------- |
| `id`             | long      | Content id (also surfaced as `externalUid`/`uid` like `tno-08949063`)                           |
| `headline`       | text      | The headline (the property is singular: `headline`, not `headlines`)                            |
| `summary`        | text/html | Short summary, may contain HTML                                                                 |
| `body`           | text      | Full text (often empty for clips)                                                               |
| `byline`         | text      | Author                                                                                          |
| `publishedOn`    | date      | UTC publish timestamp — primary sort/filter field                                               |
| `postedOn`       | date      | When ingested                                                                                   |
| `contentType`    | keyword   | `PrintContent`, `Internet`, `AudioVideo`, `Image`                                               |
| `mediaType.name` | obj       | e.g. `Daily Print`, `Online`, `News Radio`, `TV / Video News` (agg on `mediaType.name.keyword`) |
| `mediaTypeId`    | int       | numeric media type                                                                              |
| `source.name`    | text      | Outlet name (agg on `source.name.keyword`; watch leading spaces)                                |
| `source.code`    | —         | Short source code                                                                               |
| `otherSource`    | keyword   | Short outlet label, e.g. `SUN`, `GLOBE`, `POST`, `TC`, `TS`                                     |
| `series.name`    | obj       | Program/series, e.g. `CKNW`, `Global BC News Hour`                                              |
| `status`         | keyword   | `Draft`, `Published`, `Unpublished`                                                             |
| `actions`        | nested    | Editorial flags (Top Story, etc.) — see below                                                   |
| `tags`           | nested    | Ministry/subject tags — see below                                                               |
| `topics`         | nested    | Editorial topics — see below                                                                    |
| `tonePools`      | nested    | Sentiment — see below                                                                           |
| `licenseId`      | int       | Licensing bucket                                                                                |

### Actions (editorial flags) — NESTED

Each element has `name`, `value`, `valueType`, `valueLabel`, `defaultValue`. Distinct
action names and their value types:

| Action name             | valueType | Meaning                                                   |
| ----------------------- | --------- | --------------------------------------------------------- |
| `Featured Story`        | Boolean   | Flagged as a featured story                               |
| `Top Story`             | Boolean   | Flagged as a top story                                    |
| `Alert`                 | Boolean   | Flagged for alerting                                      |
| `Non Qualified Subject` | Boolean   | Editorial qualifier flag                                  |
| `Commentary`            | String    | Commentary marker (value is a string label, e.g. timeout) |

**Boolean actions are "on" when `value == "true"`** (string). To find documents where a
given action is on, match name and value within the **same** nested element:

```json
{
  "query": {
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
  }
}
```

### Tags (ministries / subjects) — NESTED

Each element has `code`, `name`, `id`, `contentId`. Tags map to BC government ministries
and subject areas. Examples (code → name):

`PSSG` → Public Safety & Solicitor General · `HLTH` → Health · `MJAG` → Attorney General ·
`ENV` → Environment and Parks · `TRAN` → Transportation and Transit ·
`JTST` → Jobs, Economic Development & Innovation · `FIN` → Finance · `FORR` → Forests ·
`EMBC` → Emergency Management · `EDU` → Education & Child Care ·
`TACZ` → Tourism, Arts, Culture & Sport · `MSD` → Social Development & Poverty Reduction ·
`TNF` → Indigenous Relations & Reconciliation · `HOZZ` → Housing and Municipal Affairs ·
`PDPE` → Premier David Eby · `LBRR` → Labour · `AGG` → Agriculture & Food ·
`MCFD` → Children & Family Development · `MHAA` → Mental Health & Addictions ·
`ECS` → Energy and Climate Solutions · `MCMX` → Mining and Critical Minerals ·
`ZHJ` → South Asian Media · `ZHX` → Chinese language media · `XRZ` → Transcripts.

`TNO` is by far the most common code; many older records have the placeholder name
`-- UPDATE MIGRATED TAG NAME --`, so filter/aggregate on `tags.code` for reliability.
Full list in `references/schema.md`.

```json
{ "query": { "nested": { "path": "tags", "query": { "term": { "tags.code": "HLTH" } } } } }
```

### Topics — NESTED

Each element has `name` (and id). Topics are editorial story-threads, e.g.
`Wildfire season`, `Trade war`, `Overdose crisis`, `Housing shortage`, `FIFA 2026`,
`Cost of living`, `Toxic drug crisis`. `Not Applicable` is the most common. Many
COVID-era topics exist (`COVID-19 vaccine`, etc.).

```json
{
  "query": {
    "nested": { "path": "topics", "query": { "match": { "topics.name": "Wildfire season" } } }
  }
}
```

### Tone pools (SENTIMENT) — NESTED

`tonePools` encodes **sentiment**. Each element has `name` (almost always `Default`),
`value` (the sentiment score), `isPublic`, `ownerId`. The scoring scale observed:

| value         | meaning                                       |
| ------------- | --------------------------------------------- |
| `> 0` (1…5)   | Positive (higher = more positive)             |
| `0`           | Neutral (most common)                         |
| `< 0` (-1…-5) | Negative (lower = more negative)              |
| `-99`         | Sentinel / not scored (exclude from analysis) |

To find negative-sentiment items (using the public Default pool):

```json
{
  "query": {
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
}
```

For a sentiment breakdown, aggregate `tonePools.value` inside a `nested` agg (remember
to exclude `-99`).

## Date filtering

`publishedOn` is UTC. The user's local zone is Pacific (Surrey, BC). For a local
"today"/"yesterday", compute the UTC span explicitly rather than relying on `now/d`,
because BC print items are stamped 07:00:00Z and broadcast items can land after UTC
midnight. A practical pattern that captures a local day's coverage:

```json
{ "range": { "publishedOn": { "gte": "2026-06-05T00:00:00Z", "lte": "2026-06-06T23:59:59Z" } } }
```

When the user just says "today", confirm the date, build the range around it, sort by
`publishedOn` desc, and note the UTC caveat if results look off.

## Recipe: Top stories for a day

Combine the nested Top-Story flag with a date range and a trimmed `_source`:

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
        { "range": { "publishedOn": { "gte": "<DAY>T00:00:00Z", "lte": "<DAY+1>T23:59:59Z" } } }
      ]
    }
  },
  "size": 100,
  "sort": [{ "publishedOn": { "order": "desc" } }]
}
```

Present results as a table with `id`, `headline`, `publishedOn`, `source`
(`otherSource` + `source.name`). More worked recipes (sentiment over time, coverage by
ministry, source breakdowns, counting) are in `references/recipes.md`.

## Output

Default to a compact table unless the user asks otherwise. Include the document `id` so
items can be looked up. When you filtered on a nested field, briefly note it (e.g. "Top
Story flag is a nested action") only if the user is debugging — otherwise just answer.
