import { act, render } from '@testing-library/react';
import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { SearchPage } from '../../../features/search-page/SearchPage';

const state = vi.hoisted(() => ({
  params: {} as { id?: string },
  filter: { search: 'fire', size: 500 } as any,
  secondaryFilter: {} as any,
  activeFilter: undefined as any,
  frontPageImageMediaTypeId: 5 as number | undefined,
  find: vi.fn(),
  getFilter: vi.fn(),
  storeFilter: vi.fn(),
  storeSearchFilter: vi.fn(),
  warn: vi.fn(),
}));
vi.mock('store/hooks', () => ({
  useApp: () => [{ requests: [] }],
  useContent: () => [
    { search: { filter: state.filter }, searchResults: { filter: state.secondaryFilter } },
    {
      findContentWithElasticsearch: state.find,
      storeSearchFilter: state.storeSearchFilter,
      validateElasticsearchQuery: vi.fn(),
    },
  ],
  useFilters: () => [{}, { getFilter: state.getFilter }],
  useSettings: () => ({ frontPageImageMediaTypeId: state.frontPageImageMediaTypeId }),
}));
vi.mock('store/slices', () => ({
  useProfileStore: () => [
    { from: 0, filter: state.activeFilter },
    { storeFrom: vi.fn(), storeFilter: state.storeFilter },
  ],
}));
vi.mock('react-router-dom', () => ({ useParams: () => state.params }));
vi.mock('react-toastify', () => ({ toast: { warn: state.warn } }));
vi.mock('features/my-searches/hooks', () => ({
  useElastic: () => (settings: any) => ({ query: { settings }, from: 0 }),
}));
vi.mock('features/utils', () => ({ castToSearchResult: (c: any) => c }));
vi.mock('components/basic-search', () => ({ BasicSearch: () => null }));
vi.mock('components/content-list', () => ({ ContentList: () => null, ViewOptions: () => null }));
vi.mock('components/date-filter', () => ({ DateFilter: () => null }));
vi.mock('components/media-type-filters', () => ({ FilterOptions: () => null }));
vi.mock('components/section', () => ({
  PageSection: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));
vi.mock('components/tool-bar', () => ({ ContentListActionBar: () => null }));
vi.mock('../../../features/search-page/components', () => ({ AdvancedSearch: () => null }));
vi.mock('../../../features/search-page/PreviousResults', () => ({ PreviousResults: () => null }));
vi.mock('../../../features/search-page/SearchPageContext', () => ({
  useSearchPageContext: () => ({ expanded: false }),
}));
vi.mock('../../../features/search-page/styled', () => ({
  SearchPage: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
}));
vi.mock('../../../features/search-page/utils', () => ({ filterFormat: (f: any) => f }));
vi.mock('tno-core', () => ({
  Col: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Row: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Show: ({ children, visible }: { children: React.ReactNode; visible?: boolean }) =>
    visible ? <div>{children}</div> : null,
  Loader: () => null,
  useWindowSize: () => ({ width: 1200 }),
}));

const noResults = { hits: { hits: [], total: { value: 0 } } };

/** Renders the page the way the app does in development, where StrictMode replays every mount effect. */
const renderPage = () =>
  render(
    <React.StrictMode>
      <SearchPage showAdvanced={false} showDate />
    </React.StrictMode>,
  );

const settle = () => act(async () => {});

describe('Search page automatic search', () => {
  beforeEach(() => {
    state.params = {};
    state.filter = { search: 'fire', size: 500 };
    state.secondaryFilter = {};
    state.activeFilter = undefined;
    state.frontPageImageMediaTypeId = 5;
    state.find.mockReset().mockResolvedValue(noResults);
    state.getFilter.mockReset();
    state.storeFilter.mockReset();
    state.storeSearchFilter.mockReset();
    state.warn.mockReset();
  });

  it('searches exactly once when the page opens, even though StrictMode replays the effects', async () => {
    renderPage();
    await settle();

    expect(state.find).toHaveBeenCalledTimes(1);
    expect(state.find).toHaveBeenCalledWith(
      expect.objectContaining({ query: { settings: state.filter } }),
      undefined,
      'search',
    );
    expect(state.warn).toHaveBeenCalledTimes(1);
  });

  it('waits for the settings before searching', async () => {
    state.frontPageImageMediaTypeId = undefined;
    const { rerender } = renderPage();
    await settle();
    expect(state.find).not.toHaveBeenCalled();

    state.frontPageImageMediaTypeId = 5;
    rerender(
      <React.StrictMode>
        <SearchPage showAdvanced={false} showDate />
      </React.StrictMode>,
    );
    await settle();

    expect(state.find).toHaveBeenCalledTimes(1);
  });

  it('searches again when the result filter buttons change, but not when the search text changes', async () => {
    const { rerender } = renderPage();
    await settle();
    expect(state.find).toHaveBeenCalledTimes(1);

    state.filter = { ...state.filter, search: 'fires' };
    rerender(
      <React.StrictMode>
        <SearchPage showAdvanced={false} showDate />
      </React.StrictMode>,
    );
    await settle();
    expect(state.find).toHaveBeenCalledTimes(1);

    state.secondaryFilter = { contentTypes: ['PrintContent'] };
    rerender(
      <React.StrictMode>
        <SearchPage showAdvanced={false} showDate />
      </React.StrictMode>,
    );
    await settle();
    expect(state.find).toHaveBeenCalledTimes(2);
  });

  it('loads a saved search before running it, and runs it once', async () => {
    state.params = { id: '7' };
    const saved = { id: 7, name: 'Saved', settings: { search: 'flood', size: 100 } };
    state.getFilter.mockResolvedValue(saved);
    const { rerender } = renderPage();
    await settle();

    expect(state.getFilter).toHaveBeenCalledWith(7);
    expect(state.storeSearchFilter).toHaveBeenCalledWith(saved.settings);
    expect(state.find).not.toHaveBeenCalled();

    // The store now holds the saved search and the active filter matches the route.
    state.filter = saved.settings;
    state.activeFilter = saved;
    rerender(
      <React.StrictMode>
        <SearchPage showAdvanced={false} showDate />
      </React.StrictMode>,
    );
    await settle();

    expect(state.find).toHaveBeenCalledTimes(1);
    expect(state.find).toHaveBeenCalledWith(
      expect.objectContaining({ query: { settings: saved.settings } }),
      undefined,
      'search',
    );
  });
});
