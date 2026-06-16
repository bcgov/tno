# `content` index — full schema reference

A `content` document is a single media-monitoring record. Below are the fields observed
in live documents, grouped by area. Types are inferred from behavior and mappings; when
a field is `text`, aggregations/exact-match usually need a `.keyword` subfield.

## Identity & core text

| Field                          | Type    | Notes                                           |
| ------------------------------ | ------- | ----------------------------------------------- |
| `id`                           | long    | Primary content id                              |
| `uid`                          | keyword | String id, e.g. `tno-08949063`                  |
| `externalUid`                  | keyword | External id (may be empty)                      |
| `headline`                     | text    | **Singular**. The headline string               |
| `summary`                      | text    | Short summary; may contain HTML markup          |
| `body`                         | text    | Full text; frequently empty for broadcast clips |
| `byline`                       | text    | Author                                          |
| `quotes`                       | array   | Extracted quotes                                |
| `page` / `section` / `edition` | text    | Print placement metadata                        |
| `sourceUrl`                    | keyword | Origin URL                                      |

## Classification

| Field                                   | Type    | Values / notes                                    |
| --------------------------------------- | ------- | ------------------------------------------------- |
| `contentType`                           | keyword | `PrintContent`, `Internet`, `AudioVideo`, `Image` |
| `mediaTypeId`                           | int     | See media-type table below                        |
| `mediaType.name`                        | text    | Agg on `mediaType.name.keyword`                   |
| `mediaType.description`                 | text    | Human description                                 |
| `mediaType.sortOrder` / `.isEnabled`    | —       | Display metadata                                  |
| `status`                                | keyword | `Draft`, `Published`, `Unpublished`               |
| `licenseId`                             | int     | Licensing bucket (1–7)                            |
| `isApproved` / `isHidden` / `isPrivate` | bool    | Visibility flags                                  |

## Source & series

| Field              | Type    | Notes                                                        |
| ------------------ | ------- | ------------------------------------------------------------ |
| `source.id`        | int     | Source id                                                    |
| `source.name`      | text    | Outlet name (agg on `.keyword`; **may have leading space**)  |
| `source.code`      | keyword | e.g. `APTN`                                                  |
| `source.shortName` | text    | e.g. `Winnipeg`                                              |
| `sourceId`         | int     | FK                                                           |
| `otherSource`      | keyword | Short label, e.g. `SUN`, `GLOBE`, `POST`, `TC`, `TS`, `APTN` |
| `series.id`        | int     | Series/program id                                            |
| `series.name`      | text    | e.g. `APTN National News`, `CKNW` (agg on `.keyword`)        |
| `seriesId`         | int     | FK                                                           |

## Dates

| Field                     | Type       | Notes                         |
| ------------------------- | ---------- | ----------------------------- |
| `publishedOn`             | date (UTC) | **Primary** sort/filter field |
| `postedOn`                | date (UTC) | Ingestion time                |
| `createdOn` / `updatedOn` | date       | Record lifecycle              |

## Ownership / audit

`owner` (object: `id`, `displayName`, `email`, `firstName`, `lastName`, `accountType`,
`preferences`), `ownerId`, `createdBy`, `updatedBy`, `version`, `timeTrackings[]`.

## Files

`fileReferences[]`: `fileName`, `path`, `s3Path`, `contentType` (MIME), `size`,
`runningTime` (ms, for AV), `isUploaded`, `isSyncedToS3`.

## NESTED objects (require `nested` queries)

### `actions[]`

`name`, `value` (text), `valueType` (`Boolean`|`String`), `valueLabel`, `defaultValue`,
`id`, `contentId`, `createdBy/On`, `updatedBy/On`, `version`.

| name                  | valueType |
| --------------------- | --------- |
| Featured Story        | Boolean   |
| Top Story             | Boolean   |
| Alert                 | Boolean   |
| Non Qualified Subject | Boolean   |
| Commentary            | String    |

Boolean "on" = `value == "true"` (string compare via `match`).

### `tags[]`

`code`, `name`, `id`, `contentId`. Aggregate on `tags.code` (reliable) — many records
have the placeholder name `-- UPDATE MIGRATED TAG NAME --`.

### `topics[]`

`name`, `id`. Editorial story-threads.

### `tonePools[]` (SENTIMENT)

`name` (≈always `Default`), `value` (int sentiment score), `isPublic`, `ownerId`,
`createdBy/On`, `updatedBy/On`, `version`. Scale: positive `1..5`, neutral `0`,
negative `-1..-5`, `-99` = sentinel/unscored.

### `notifications[]`, `userNotifications[]`, `tonePools.versions`

Auxiliary; rarely queried.

---

## Media types (`mediaTypeId` → `mediaType.name`)

| id  | name               |
| --- | ------------------ |
| 1   | Daily Print        |
| 2   | Weekly Print       |
| 3   | Online             |
| 4   | News Radio         |
| 5   | Talk Radio         |
| 6   | TV / Video News    |
| 7   | CP Wire            |
| 8   | AV Archive         |
| 9   | Events             |
| 10  | Front Page Images  |
| 11  | Corporate Calendar |

## Content types

`PrintContent`, `Internet`, `AudioVideo`, `Image`.

## Status

`Draft`, `Published`, `Unpublished`. (Drafts outnumber Published — filter to
`Published` for public/published views.)

---

## Tags — code → ministry/subject (most common)

| code | name                                    |
| ---- | --------------------------------------- |
| TNO  | (migrated/placeholder; most common)     |
| PSSG | Public Safety & Solicitor General       |
| HLTH | Health                                  |
| MJAG | Attorney General                        |
| ZPZ  | Key players                             |
| ENV  | Environment and Parks                   |
| TRAN | Transportation and Transit              |
| JTST | Jobs, Economic Development & Innovation |
| FIN  | Finance                                 |
| FORR | Forests                                 |
| EMBC | Emergency Management                    |
| CSC  | (subject)                               |
| MAZ  | (subject)                               |
| EDU  | Education & Child Care                  |
| TACZ | Tourism, Arts, Culture & Sport          |
| MSD  | Social Development & Poverty Reduction  |
| XRZ  | Transcripts                             |
| TNF  | Indigenous Relations & Reconciliation   |
| HOZZ | Housing and Municipal Affairs           |
| PDPE | Premier David Eby                       |
| LBRR | Labour                                  |
| AGG  | Agriculture & Food                      |
| MCFD | Children & Family Development           |
| ZHJ  | South Asian Media                       |
| ADV  | (subject)                               |
| ABO  | (subject)                               |
| EMPR | (subject)                               |
| MHAA | Mental Health & Addictions              |
| ECS  | Energy and Climate Solutions            |
| MCMX | Mining and Critical Minerals            |
| ZHX  | Chinese language media                  |
| LWRS | Land, Water & Resource Stewardship      |
| BCFF | BC Ferries                              |
| INFX | Infrastructure                          |
| SSS  | Scrums & Speeches                       |
| ICBC | ICBC                                    |
| CITZ | Citizens' Services                      |
| ZZA  | Seniors                                 |
| IGR  | Intergovernmental Relations Secretariat |

(Other codes appear with lower frequency; query `tags.code` aggregation for the live
list. The `name` field is unreliable for older records.)

## Sources (highest-volume outlets)

Canadian Press Wire, National Post, Victoria Times Colonist, Vancouver Sun, The
Province, CKNW (Corus), Tri-City News, Burnaby Now, Kelowna Courier, New Westminster
Record, Prince George Citizen, Abbotsford News, Castanet, Global TV, CHNL (Stingray),
The Daily News (Nanaimo/Kamloops), Vernon Morning Star, Richmond News Online, Daily
Hive, CBC (Vancouver/Online/Victoria/Prince George/Kelowna), CFAX (Bell), Globe and
Mail, Edmonton Journal, Calgary Herald, Toronto Sun, Business in Vancouver, and a long
tail of BC community papers and broadcasters (~hundreds total).

Aggregate `source.name.keyword` for the current ranked list. Short labels live in
`otherSource` (`SUN`, `GLOBE`, `POST`, `TC`, `TS`, …).
