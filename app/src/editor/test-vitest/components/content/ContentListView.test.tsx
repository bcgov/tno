import { render, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import ContentListView from '@/editor/features/content/list-view/ContentListView';
import { ContentStatusName, ContentTypeName } from '@/shared';

import { TestWrapper } from '../../utils';

// Ported from the Jest test that sat beside the component and never ran under Vitest.

const content = {
  id: 1,
  headline: 'test headline',
  owner: { displayName: 'user@idir', username: 'test@idir' },
  otherSource: 'TEST',
  publishedOn: '2022-05-12T16:11:15.756251Z',
  createdOn: '2022-05-12T16:11:15.756251Z',
  contentType: ContentTypeName.AudioVideo,
  status: ContentStatusName.Publish,
  mediaType: { name: 'News' },
  page: 'A32',
};

const { findContentWithElasticsearch } = vi.hoisted(() => ({
  findContentWithElasticsearch: vi.fn(),
}));
const { initialContentState } = await vi.hoisted(
  async () => await import('@/editor/store/slices/content/contentSlice'),
);
const lookupOptions = {
  mediaTypeOptions: [],
  sourceOptions: [],
  seriesOptions: [],
  series: [],
  userOptions: [],
  ingestTypeOptions: [],
  licenseOptions: [],
  users: [],
};

vi.mock('@/editor/store/hooks', () => ({
  useSettings: () => ({ isReady: true }),
  useApp: () => [{ requests: [], userInfo: { id: 1 } }, {}],
  useContent: () => [
    initialContentState,
    {
      getContent: vi.fn(),
      storeFilter: vi.fn(),
      storeFilterAdvanced: vi.fn(),
      storeSearchResults: vi.fn(),
      findContentWithElasticsearch,
      updateContent: vi.fn(),
    },
  ],
  useApiHub: () => ({ useHubEffect: vi.fn() }),
  useLocalStorage: (_key: string, value: unknown) => [value, vi.fn()],
  useWorkOrders: () => [{}, { findWorkOrders: vi.fn().mockResolvedValue({ items: [] }) }],
  useLookup: () => [{ mediaTypes: [], sources: [], users: [], ingestTypes: [] }, {}],
  useLookupOptions: () => [lookupOptions, {}],
}));

describe('ContentListView', () => {
  beforeEach(() => {
    findContentWithElasticsearch.mockReset();
    findContentWithElasticsearch.mockResolvedValue({
      hits: { hits: [{ _source: content }], total: { value: 1, relation: 'eq' } },
    });
  });

  it('renders both advanced search rows', async () => {
    render(
      <TestWrapper>
        <ContentListView />
      </TestWrapper>,
    );
    await waitFor(() => expect(screen.getAllByTestId(/advanced-search-row-/)).toHaveLength(2));
  });

  it.each(['Headline', 'Source', 'Media Type', 'User', 'Pub Date', 'Use'])(
    'renders the %s column',
    async (column) => {
      render(
        <TestWrapper>
          <ContentListView />
        </TestWrapper>,
      );
      await waitFor(() => expect(screen.getAllByText(column).length).toBeGreaterThan(0));
    },
  );
});
