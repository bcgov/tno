import { render } from '@testing-library/react';
import { ContentListView } from './ContentListView';
import { mockContent, TestWrapper } from 'test/utils';

jest.mock('store', () => ({
  useAppDispatch: jest.fn(),
  useAppSelector: jest.fn(),
}));

const mockUseLookups = [
  {
    contentTypes: [{ name: 'Content 1', id: 1 }],
    mediaTypes: [{ name: 'Media 1', id: 1 }],
    users: [{ name: 'user', id: 1, displayName: 'displayName', username: 'test' }],
  },
];

const mockUseContent = [
  {
    filter: {},
    filterAdvanced: {},
    content: {
      items: [
        {
          headline: 'test headline',
          owner: { displayName: 'user@idir', username: 'test@idir' },
          source: 'TEST',
          createdOn: '2022-05-12T16:11:15.756251Z',
          mediaType: {
            name: 'Television'
          },
          status: 'Publish',
          page: 'A32'
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
  { findContent: () => mockContent, storeFilter: {} },
];

const mockUseApp = [
  {
    userInfo: '',
    requests: [],
  },
  { isUserReady: () => true },
];

jest.mock('store/hooks', () => ({
  useApp: () => mockUseApp,
  useContent: () => mockUseContent,
  useLookup: () => mockUseLookups,
}));

it('renders correctly...', () => {
  const { container } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );

  expect(container).toMatchSnapshot();
});

it('displays username without idir tag', () => {
  const { getByText, queryByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  expect(getByText('user')).toBeInTheDocument();
  expect(queryByText('@idir')).toBeFalsy();
});

it('displays the date in the correct format', () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  expect(getByText('05/12/2022')).toBeInTheDocument();
});

it('displays the source', () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  expect(getByText(/TEST/)).toBeInTheDocument();
});

it('displays the media type', () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  expect(getByText('Television')).toBeInTheDocument();
});

it('displays the status', () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  expect(getByText('Publish')).toBeInTheDocument();
});

it('displays the section/page', () => {
  const { getByText } = render(
    <TestWrapper>
      <ContentListView />
    </TestWrapper>,
  );
  expect(getByText('A32')).toBeInTheDocument();
});