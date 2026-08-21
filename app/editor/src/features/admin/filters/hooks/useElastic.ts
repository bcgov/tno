import React from 'react';
import { useSettings } from 'store/hooks';
import { generateMustNotQuery, generateQuery, type IFilterSettingsModel } from 'tno-core';

/**
 * Hook provides helper function to generate elasticsearch query.
 * It also removes front page images from any query that isn't specifically asking for them.
 * @returns Function to generate an elasticsearch query from configuration.
 */
export const useElastic = () => {
  const { frontpageImageMediaTypeId } = useSettings();

  return React.useCallback(
    (
      filter: IFilterSettingsModel,
      query?: any,
      condition: 'must' | 'must_not' | 'filter' = 'must',
    ) => {
      if (!frontpageImageMediaTypeId) return;

      const elastic = generateQuery(filter, query, condition);

      // The front-page-images exclusion is owned entirely by this hook. It must be reconciled on
      // every pass: added when front page images are not specifically requested, and removed when
      // they are. generateMustQuery preserves any prior 'must_not', so without stripping it here a
      // previously-baked-in exclusion is carried forward forever and selecting "Front Page Images"
      // would never un-hide them.
      const { must_not: priorMustNot, ...restBool } = elastic.query?.bool ?? {};
      const existingMustNot: any[] = Array.isArray(priorMustNot)
        ? priorMustNot
        : priorMustNot
        ? [priorMustNot]
        : [];

      // Drop only the front-page-media-type exclusion this hook may have added before; leave any
      // other must_not clauses untouched.
      const isFrontpageExclusion = (clause: any) => {
        const ids = clause?.terms?.mediaTypeId;
        return Array.isArray(ids) && ids.length === 1 && ids[0] === frontpageImageMediaTypeId;
      };
      let mustNot = existingMustNot.filter((c) => !isFrontpageExclusion(c));

      if (!filter.mediaTypeIds?.includes(frontpageImageMediaTypeId)) {
        // Do not include front page images in results unless they are specifically requested.
        const frontpageMustNot = generateMustNotQuery({
          mediaTypeIds: [frontpageImageMediaTypeId],
        }).query?.bool?.must_not;
        if (Array.isArray(frontpageMustNot)) mustNot = [...mustNot, ...frontpageMustNot];
        else if (frontpageMustNot) mustNot = [...mustNot, frontpageMustNot];
      }

      // Body length constraints run as Elasticsearch runtime fields computed from _source, so
      // they need no mapping change or reindex. Reconciled on every pass like the front-page
      // exclusion: prior body-length clauses/runtime fields are stripped and re-added from the
      // current settings.
      const extended = filter as IFilterSettingsModel & {
        minBodyLength?: number;
        minBodyWords?: number;
      };
      const { runtime_mappings: priorRuntime, ...elasticRest } = elastic as Record<string, any>;
      const runtimeMappings: Record<string, unknown> = { ...(priorRuntime ?? {}) };
      delete runtimeMappings.body_length;
      delete runtimeMappings.body_words;
      const lengthClauses: unknown[] = [];
      if (extended.minBodyLength && extended.minBodyLength > 0) {
        runtimeMappings.body_length = {
          type: 'long',
          script: { source: 'def b = params._source["body"]; emit(b == null ? 0 : b.length())' },
        };
        lengthClauses.push({ range: { body_length: { gte: extended.minBodyLength } } });
      }
      if (extended.minBodyWords && extended.minBodyWords > 0) {
        runtimeMappings.body_words = {
          type: 'long',
          script: {
            source:
              'def b = params._source["body"]; emit(b == null ? 0 : b.splitOnToken(" ").length)',
          },
        };
        lengthClauses.push({ range: { body_words: { gte: extended.minBodyWords } } });
      }
      const priorFilter = (restBool as Record<string, any>).filter;
      const filterClauses: any[] = Array.isArray(priorFilter)
        ? priorFilter
        : priorFilter
        ? [priorFilter]
        : [];
      const isLengthClause = (clause: any) =>
        clause?.range?.body_length != null || clause?.range?.body_words != null;
      const nextFilter = [...filterClauses.filter((c) => !isLengthClause(c)), ...lengthClauses];
      const { filter: _priorFilter, ...restBoolNoFilter } = restBool as Record<string, any>;

      return {
        ...elasticRest,
        ...(Object.keys(runtimeMappings).length ? { runtime_mappings: runtimeMappings } : {}),
        query: {
          ...elastic.query,
          bool: {
            ...restBoolNoFilter,
            ...(nextFilter.length ? { filter: nextFilter } : {}),
            ...(mustNot.length ? { must_not: mustNot } : {}),
          },
        },
      };
    },
    [frontpageImageMediaTypeId],
  );
};
