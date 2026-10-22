import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';

/**
 * Sets or removes the query's collapse, which returns one story per distinct value of a field.
 * The query is rebuilt from settings on every change, so a collapse from an earlier pass is
 * removed when the setting is cleared.
 * @param elastic The query generated from the settings.
 * @param field The field to collapse on.
 * @returns Elasticsearch query JSON.
 */
export const generateCollapse = (
  elastic: MsearchMultisearchBody,
  field?: string,
): MsearchMultisearchBody => {
  const { collapse, ...rest } = elastic as any;
  return field ? { ...rest, collapse: { field } } : rest;
};
