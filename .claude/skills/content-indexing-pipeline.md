# Skill: Content Indexing Pipeline (Debugging)

## Pipeline Flow

Content flows: **syndication** (or other ingest service) → API `/api/kafka/producers/content/<SOURCE>`
→ Kafka `content` topic → **content** service (creates content rows via API) → Kafka `index` topic
→ **indexing** service → Elasticsearch (`unpublished_content` / `content` aliases).

When "content isn't showing up in Elasticsearch", walk the stages in order:

```bash
docker logs tno-syndication --since 10m | grep -iE "producers/content|error|verification result"
docker logs tno-content     --since 10m | grep -iE "error|fail" | head
docker logs tno-indexing    --since 10m | grep -iE "Indexing content|error|fail" | head
```

Then compare the newest content in Postgres vs Elasticsearch — a gap pinpoints where it broke:

```bash
docker exec tno-database psql -U admin -d tno -t -c "SELECT max(id), count(*) FROM content;"
curl -s -u 'elastic:<ELASTIC_PASSWORD from api/net/.env>' \
  "http://localhost:40003/unpublished_content/_search?size=1&sort=id:desc&_source=id"
```

Note: `_cat/indices` `docs.count` counts nested Lucene docs — use `_search` `hits.total` for the
real top-level document count.

## Known Failure: Indexing 401 Against Elasticsearch

Symptom: indexing logs `Could not authenticate with the specified node ... missing authentication
credentials for REST request`, retries 3x per message, nothing lands in ES.

Cause: `services/net/indexing/.env` generated with `Service__ElasticsearchUri/Username/Password`
keys — **no code reads those**. `IndexingManager` binds the `Elastic` config section, so the env
vars must be:

```
Elastic__Url=http://elastic:9200
Elastic__Username=elastic
Elastic__Password=<ELASTIC_PASSWORD>
```

After fixing, recreate the container (not `docker restart` — see `docker-compose-services` skill).
Kafka replays uncommitted `index` messages on restart, so the backlog self-heals; confirm the ES
max id catches up to the Postgres max id.

## Other Pipeline Blockers Seen Locally

- Service-account 401s against the API (every service calls the API): usually the Keycloak issuer
  list or the placeholder client secret — see the `keycloak-implementation` skill.
- Syndication logs `schedule [X] verification result: False` — the ingest's schedule says "not
  now"; it is not an error.
- Sorting on nested fields (`source`, `mediaType`, `owner`, `series`) of a freshly migrated empty
  index returns ES 400 → API 500, because those mappings only appear via dynamic mapping when the
  first document is indexed (see CLAUDE.md Elasticsearch notes).
