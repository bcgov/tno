import { EXTENDED_QUERY_NAME, generateQuery, type IFilterSettingsModel } from 'tno-core';

const extendedQuery = {
  should: [{ match: { headline: 'nurse' } }, { match: { headline: 'doctor' } }],
  minimum_should_match: 1,
};

const namedClauses = (query: any) =>
  (query.query?.bool?.filter ?? []).filter((c: any) => c?.bool?._name === EXTENDED_QUERY_NAME);

describe('generateQuery with an extended query', () => {
  it('adds the extended query as one named bool filter clause', () => {
    const settings: IFilterSettingsModel = {
      searchUnpublished: false,
      size: 10,
      sourceIds: [1],
      extendedQuery,
    };

    const query: any = generateQuery(settings, {});

    expect(namedClauses(query)).toEqual([
      { bool: { ...extendedQuery, _name: EXTENDED_QUERY_NAME } },
    ]);
    expect(query.query.bool.must).toEqual([{ terms: { sourceId: [1] } }]);
    expect(query.query.match_all).toBeUndefined();
  });

  it('keeps exactly one clause when the query is generated again from its own output', () => {
    const settings: IFilterSettingsModel = {
      searchUnpublished: false,
      size: 10,
      extendedQuery,
    };

    const once: any = generateQuery(settings, {});
    const twice: any = generateQuery({ ...settings, sourceIds: [2] }, once);
    const thrice: any = generateQuery({ ...settings, dateOffset: 7 }, twice);

    expect(namedClauses(twice)).toHaveLength(1);
    expect(namedClauses(thrice)).toHaveLength(1);
    expect(thrice.query.bool.filter).toHaveLength(2);
  });

  it('replaces a match_all placeholder rather than adding a second root clause', () => {
    const query: any = generateQuery(
      { searchUnpublished: false, size: 10, extendedQuery },
      { query: { match_all: {} } },
    );

    expect(Object.keys(query.query)).toEqual(['bool']);
    expect(namedClauses(query)).toHaveLength(1);
  });

  it('removes the clause when the extended query is removed', () => {
    const settings: IFilterSettingsModel = { searchUnpublished: false, size: 10, extendedQuery };
    const withClause: any = generateQuery(settings, {});

    const without: any = generateQuery({ ...settings, extendedQuery: undefined }, withClause);

    expect(namedClauses(without)).toHaveLength(0);
    expect(without.query).toEqual({ match_all: {} });
  });
});

describe('generateQuery with a collapse field', () => {
  it('collapses on the field and removes the collapse when it is cleared', () => {
    const settings: IFilterSettingsModel = {
      searchUnpublished: false,
      size: 10,
      dateOffset: 0,
      collapseField: 'analysis.primaryTopic',
    };

    const collapsed: any = generateQuery(settings, {});
    const cleared: any = generateQuery({ ...settings, collapseField: undefined }, collapsed);

    expect(collapsed.collapse).toEqual({ field: 'analysis.primaryTopic' });
    expect(cleared.collapse).toBeUndefined();
  });
});
