import { render, waitFor } from '@testing-library/react';
import { AdvancedSearchKeys } from 'features/content/constants';
import { mockContent, TestWrapper } from 'test/utils';
import { ContentStatusName, ContentTypeName } from 'tno-core';

import ContentListView from './ContentListView';

jest.mock('store', () => ({
  useAppDispatch: jest.fn(),
  useAppSelector: jest.fn(),
}));

const mockUseLookups = [
  {
    mediaTypes: [{ name: 'Media Type 1', id: 1 }],
    licenses: [{ name: 'License 1', id: 1 }],
    ingestTypes: [{ name: 'Ingest 1', id: 1 }],
    users: [{ name: 'user', id: 1, displayName: 'displayName', username: 'test' }],
  },
];

const mockUseLookupOptions = [
  {
    mediaTypes: [{ name: 'Media Type 1', id: 1 }],
    licenses: [{ name: 'License 1', id: 1 }],
    ingestTypes: [{ name: 'Ingest 1', id: 1 }],
    users: [{ name: 'user', id: 1, displayName: 'displayName', username: 'test' }],
    ingestTypeOptions: [],
    mediaTypeOptions: [],
    sourceOptions: [],
    seriesOptions: [],
    series: [],
    userOptions: [],
  },
];

const mockUseContent = [
  {
    filter: {},
    filterAdvanced: {
      fieldType: AdvancedSearchKeys.Source,
      logicalOperator: '',
      searchTerm: '',
      secondaryFieldType: AdvancedSearchKeys.Source,
      secondaryLogicalOperator: '',
      secondarySearchTerm: '',
    },
    content: {
      items: [
        {
          headline: 'test headline',
          owner: { displayName: 'user@idir', username: 'test@idir' },
          otherSource: 'TEST',
          createdOn: '2022-05-12T16:11:15.756251Z',
          contentType: ContentTypeName.AudioVideo,
          status: ContentStatusName.Publish,
          mediaType: {
            name: 'News',
          },
          page: 'A32',
        },
      ],
      page: 1,
      total: 1,
      quantity: 10,
    },
    page: 1,
    total: 1,
    quantity: 10,
  },
  { findContent: () => Promise.resolve(mockContent), storeFilter: {} },
];

const mockUseApp = [
  {
    userInfo: '',
    requests: [],
  },
  {},
];

jest.mock('hooks', () => ({
  useLookupOptions: () => mockUseLookupOptions,
}));

jest.mock('store/hooks', () => ({
  useApp: () => mockUseApp,
  useContent: () => mockUseContent,
  useLookup: () => mockUseLookups,
}));

it('renders advanced search rows', async () => {
  const { getAllByTestId } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  await waitFor(() => {
    const rows = getAllByTestId(/advanced-search-row-/);
    expect(rows.length).toBe(2);
  });
});

it('displays username without idir tag', async () => {
  const { getByText, queryByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  await waitFor(() => {
    expect(getByText('user')).toBeInTheDocument();
    expect(queryByText('@idir')).toBeFalsy();
  });
});

it('displays the date in the correct format', async () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  await waitFor(() => expect(getByText('05/12/2022')).toBeInTheDocument());
});

it('displays the Source column', async () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  await waitFor(() => expect(getByText('Source')).toBeInTheDocument());
});

it('displays the Media Type column', async () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  await waitFor(() => expect(getByText('Media Type')).toBeInTheDocument());
});

it('displays the status column', async () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  await waitFor(() => expect(getByText('Pub Date')).toBeInTheDocument());
});

it('displays the Use column', async () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  await waitFor(() => expect(getByText('Use')).toBeInTheDocument());
});
