import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import OrphanedContent from 'features/admin/orphaned-content/OrphanedContent';
import React from 'react';
import { TestWrapper } from 'test/utils';
import { vi } from 'vitest';

const api = vi.hoisted(() => ({
  latest: vi.fn(),
  start: vi.fn(),
  cancel: vi.fn(),
  results: vi.fn(),
  resolve: vi.fn(),
}));
vi.mock('store/hooks/admin/useOrphanedContent', () => ({ useOrphanedContent: () => api }));
const completed = {
  id: 'scan-1',
  side: 'elasticsearch',
  index: 'content',
  status: 'Completed',
  scanned: 1000000,
  pending: 0,
  found: 1,
  cancelRequested: false,
  createdOn: '2026-10-25T12:00:00Z',
  updatedOn: '2026-10-25T13:00:00Z',
  completedOn: '2026-10-25T13:00:00Z',
};
const orphan = { id: 700, headline: 'Orphan story', status: 'Published', resolution: '' };
const renderPage = () =>
  render(
    <TestWrapper>
      <OrphanedContent />
    </TestWrapper>,
  );

beforeEach(() => {
  Object.values(api).forEach((fn) => fn.mockReset());
  api.latest.mockResolvedValue(null);
  api.results.mockResolvedValue({ items: [orphan], nextAfter: 700, hasMore: false });
});

afterEach(() => vi.restoreAllMocks());

it.each([
  ['2026-10-26T03:00:00Z', '2026-09-25', '2026-10-25'],
  ['2026-11-02T07:30:00Z', '2026-10-02', '2026-11-01'],
])('defaults new scans to the last 30 Pacific calendar days at %s', async (now, start, end) => {
  vi.spyOn(Date, 'now').mockReturnValue(new Date(now).getTime());
  const user = userEvent.setup();
  api.start.mockResolvedValue({ ...completed, startPostedOn: start, endPostedOn: end });
  renderPage();
  await waitFor(() => expect(screen.getByRole('button', { name: 'Start new scan' })).toBeEnabled());
  expect(screen.getByLabelText('Start posted date (optional)')).toHaveValue(start);
  expect(screen.getByLabelText('End posted date (optional)')).toHaveValue(end);
  await user.click(screen.getByRole('button', { name: 'Start new scan' }));
  expect(api.start).toHaveBeenCalledWith('elasticsearch', 'content', start, end);
});

it('keeps previous scan dates in the results summary without replacing new-scan defaults', async () => {
  vi.spyOn(Date, 'now').mockReturnValue(new Date('2026-10-26T03:00:00Z').getTime());
  api.latest.mockResolvedValue({
    ...completed,
    startPostedOn: '2099-01-01',
    endPostedOn: '2099-01-01',
  });
  renderPage();
  await waitFor(() =>
    expect(screen.getByRole('status')).toHaveTextContent(
      'Posted dates (Pacific): 2099-01-01 through 2099-01-01',
    ),
  );
  expect(screen.getByLabelText('Start posted date (optional)')).toHaveValue('2026-09-25');
  expect(screen.getByLabelText('End posted date (optional)')).toHaveValue('2026-10-25');
});

it('starts a background scan once and displays persisted progress', async () => {
  const user = userEvent.setup();
  const running = { ...completed, status: 'Scanning', completedOn: null, found: 0 };
  api.start.mockImplementation(async () => {
    api.latest.mockResolvedValue(running);
    return running;
  });
  renderPage();
  await waitFor(() => expect(screen.getByRole('button', { name: 'Start new scan' })).toBeEnabled());
  fireEvent.change(screen.getByLabelText('Start posted date (optional)'), {
    target: { value: '2026-10-01' },
  });
  fireEvent.change(screen.getByLabelText('End posted date (optional)'), {
    target: { value: '2026-10-25' },
  });
  await user.click(screen.getByRole('button', { name: 'Start new scan' }));
  await waitFor(() =>
    expect(screen.getByRole('status')).toHaveTextContent('1,000,000 records checked'),
  );
  expect(api.start).toHaveBeenCalledTimes(1);
  expect(api.start).toHaveBeenCalledWith('elasticsearch', 'content', '2026-10-01', '2026-10-25');
  expect(screen.getByRole('button', { name: 'Start new scan' })).toBeDisabled();
  expect(screen.queryByRole('button', { name: 'Continue scan' })).not.toBeInTheDocument();
});

it('reconnects to an existing scan and requests cancellation', async () => {
  const user = userEvent.setup();
  const running = { ...completed, status: 'Confirming', completedOn: null, pending: 50 };
  api.latest.mockResolvedValue(running);
  api.cancel.mockImplementation(async () => {
    api.latest.mockResolvedValue({ ...running, cancelRequested: true });
  });
  renderPage();
  await user.click(await screen.findByRole('button', { name: 'Cancel scan' }));
  expect(api.cancel).toHaveBeenCalledWith('scan-1');
  expect(api.start).not.toHaveBeenCalled();
  await waitFor(() => expect(screen.getByRole('button', { name: 'Cancel scan' })).toBeDisabled());
});

it('replaces the results page instead of accumulating records in memory', async () => {
  const user = userEvent.setup();
  api.latest.mockResolvedValue(completed);
  api.results.mockImplementation(async (_id, after) =>
    after === 0
      ? { items: [orphan], nextAfter: 700, hasMore: true }
      : {
          items: [{ ...orphan, id: 800, headline: 'Second page' }],
          nextAfter: 800,
          hasMore: false,
        },
  );
  renderPage();
  expect(await screen.findByText('Orphan story')).toBeInTheDocument();
  await user.click(screen.getByRole('checkbox', { name: 'Select content 700' }));
  expect(screen.getByRole('button', { name: 'Sync all' })).toBeEnabled();
  await user.click(screen.getByRole('button', { name: 'Next results page' }));
  expect(await screen.findByText('Second page')).toBeInTheDocument();
  expect(screen.getByRole('button', { name: 'Sync all' })).toBeDisabled();
  expect(screen.getByRole('checkbox', { name: 'Select content 800' })).not.toBeChecked();
  expect(screen.queryByText('Orphan story')).not.toBeInTheDocument();
  expect(api.results).toHaveBeenLastCalledWith('scan-1', 700);
  await user.click(screen.getByRole('button', { name: 'First results page' }));
  expect(await screen.findByText('Orphan story')).toBeInTheDocument();
  expect(screen.getByRole('checkbox', { name: 'Select content 700' })).not.toBeChecked();
});

it('selects individual rows or all unresolved rows on the page and uses accessible icon actions', async () => {
  const user = userEvent.setup();
  api.latest.mockResolvedValue({ ...completed, side: 'postgres' });
  api.results.mockResolvedValue({
    items: [orphan, { ...orphan, id: 701 }, { ...orphan, id: 702, resolution: 'Indexing queued' }],
    nextAfter: 702,
    hasMore: false,
  });
  renderPage();
  const first = await screen.findByRole('checkbox', { name: 'Select content 700' });
  const second = screen.getByRole('checkbox', { name: 'Select content 701' });
  const resolved = screen.getByRole('checkbox', { name: 'Select content 702' });
  const all = screen.getByRole('checkbox', { name: 'Select all on this page' });
  const sync = screen.getByRole('button', { name: 'Sync all' });
  expect(sync).toBeDisabled();
  expect(resolved).toBeDisabled();
  expect(screen.getAllByRole('button', { name: 'Add to Elasticsearch' })[0].textContent).toBe('');
  expect(screen.getAllByRole('button', { name: 'Delete content' })[0].textContent).toBe('');
  await user.click(first);
  expect(sync).toBeEnabled();
  expect(all).toBePartiallyChecked();
  await user.click(all);
  expect(first).toBeChecked();
  expect(second).toBeChecked();
  expect(all).toBeChecked();
  expect(resolved).not.toBeChecked();
  expect(screen.getByText('2 selected on this page')).toBeInTheDocument();
  await user.click(all);
  expect(first).not.toBeChecked();
  expect(second).not.toBeChecked();
  expect(sync).toBeDisabled();
  expect(api.resolve).not.toHaveBeenCalled();
});

it('syncs selected items sequentially, continues after failure, and retains only failures for retry', async () => {
  const user = userEvent.setup();
  api.latest.mockResolvedValue({ ...completed, side: 'postgres', index: 'published' });
  let rows = [orphan, { ...orphan, id: 701 }, { ...orphan, id: 702 }];
  api.results.mockImplementation(async () => ({ items: rows, nextAfter: 702, hasMore: false }));
  let finishFirst!: () => void;
  const firstPending = new Promise<void>((resolve) => {
    finishFirst = resolve;
  });
  let failSecond = true;
  api.resolve.mockImplementation(async (id) => {
    if (id === 700) await firstPending;
    if (id === 701 && failSecond) throw new Error('Indexing unavailable');
    rows = rows.map((row) => (row.id === id ? { ...row, resolution: 'Indexing queued' } : row));
    return { message: 'Indexing queued.' };
  });
  renderPage();
  await user.click(await screen.findByRole('checkbox', { name: 'Select all on this page' }));
  await user.click(screen.getByRole('button', { name: 'Sync all' }));
  expect(api.resolve).not.toHaveBeenCalled();
  await user.click(screen.getByRole('button', { name: 'Confirm', hidden: true }));
  expect(api.resolve).toHaveBeenCalledTimes(1);
  expect(screen.getByRole('button', { name: 'Sync all' })).toBeDisabled();
  expect(screen.getByRole('checkbox', { name: 'Select all on this page' })).toBeDisabled();
  finishFirst();
  await waitFor(() =>
    expect(screen.getByRole('status')).toHaveTextContent(
      '2 items queued for indexing. 1 item failed and remains selected for retry.',
    ),
  );
  expect(api.resolve.mock.calls).toEqual([
    [700, 'index', 'postgres', 'published', 'scan-1'],
    [701, 'index', 'postgres', 'published', 'scan-1'],
    [702, 'index', 'postgres', 'published', 'scan-1'],
  ]);
  expect(screen.getByRole('checkbox', { name: 'Select content 700' })).not.toBeChecked();
  expect(screen.getByRole('checkbox', { name: 'Select content 701' })).toBeChecked();
  expect(screen.getByRole('checkbox', { name: 'Select content 702' })).not.toBeChecked();
  expect(screen.getByRole('checkbox', { name: 'Select content 700' })).toBeDisabled();
  failSecond = false;
  await user.click(screen.getByRole('button', { name: 'Sync all' }));
  await user.click(screen.getByRole('button', { name: 'Confirm', hidden: true }));
  await waitFor(() =>
    expect(screen.getByRole('status')).toHaveTextContent('1 item queued for indexing.'),
  );
  expect(api.resolve).toHaveBeenCalledTimes(4);
  expect(api.resolve).toHaveBeenLastCalledWith(701, 'index', 'postgres', 'published', 'scan-1');
  expect(screen.getByRole('button', { name: 'Sync all' })).toBeDisabled();
});

it('restores only selected Elasticsearch content using the saved scan comparison', async () => {
  const user = userEvent.setup();
  api.latest.mockResolvedValue(completed);
  let rows = [orphan, { ...orphan, id: 701 }];
  api.results.mockImplementation(async () => ({ items: rows, nextAfter: 701, hasMore: false }));
  api.resolve.mockImplementation(async (id) => {
    rows = rows.map((row) => (row.id === id ? { ...row, resolution: 'Restored' } : row));
    return { message: 'Content restored to PostgreSQL.' };
  });
  renderPage();
  await user.click(await screen.findByRole('checkbox', { name: 'Select content 701' }));
  await user.click(screen.getByRole('button', { name: 'Sync all' }));
  expect(api.resolve).not.toHaveBeenCalled();
  expect(screen.getByText(/Restoration cannot recover deleted files/)).toBeInTheDocument();
  await user.click(screen.getByRole('button', { name: 'Confirm', hidden: true }));
  await waitFor(() =>
    expect(screen.getByRole('status')).toHaveTextContent('1 item restored to PostgreSQL.'),
  );
  expect(api.resolve.mock.calls).toEqual([[701, 'restore', 'elasticsearch', 'content', 'scan-1']]);
  expect(screen.getByRole('checkbox', { name: 'Select content 700' })).toBeEnabled();
  expect(screen.getByRole('button', { name: 'Sync all' })).toBeDisabled();
});

it('requires confirmation and keeps the result when repair is rejected', async () => {
  const user = userEvent.setup();
  api.latest.mockResolvedValue(completed);
  api.resolve.mockRejectedValue(new Error('No longer orphaned'));
  renderPage();
  await user.click(await screen.findByRole('button', { name: 'Delete content' }));
  expect(api.resolve).not.toHaveBeenCalled();
  await user.click(screen.getByRole('button', { name: 'Confirm', hidden: true }));
  await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('action failed'));
  expect(api.resolve).toHaveBeenCalledWith(700, 'delete', 'elasticsearch', 'content', 'scan-1');
  expect(screen.getByText('Orphan story')).toBeInTheDocument();
});

it('clears the previous selection when starting a new scan', async () => {
  const user = userEvent.setup();
  api.latest.mockResolvedValue(completed);
  api.start.mockImplementation(async () => {
    const next = { ...completed, id: 'scan-2', status: 'Scanning', completedOn: null };
    api.latest.mockResolvedValue(next);
    return next;
  });
  renderPage();
  await user.click(await screen.findByRole('checkbox', { name: 'Select content 700' }));
  expect(screen.getByRole('button', { name: 'Sync all' })).toBeEnabled();
  await user.click(screen.getByRole('button', { name: 'Start new scan' }));
  await screen.findByRole('checkbox', { name: 'Select content 700' });
  expect(screen.getByRole('button', { name: 'Sync all' })).toBeDisabled();
  expect(screen.getByRole('checkbox', { name: 'Select content 700' })).not.toBeChecked();
  expect(api.resolve).not.toHaveBeenCalled();
});

it('shows persisted indexing requests as queued and prevents repeated repair', async () => {
  const user = userEvent.setup();
  api.latest.mockResolvedValue({ ...completed, side: 'postgres' });
  api.resolve.mockImplementation(async () => {
    api.results.mockResolvedValue({
      items: [{ ...orphan, resolution: 'Indexing queued' }],
      nextAfter: 700,
      hasMore: false,
    });
    return { message: 'Indexing queued.' };
  });
  renderPage();
  await user.click(await screen.findByRole('button', { name: 'Add to Elasticsearch' }));
  await user.click(screen.getByRole('button', { name: 'Confirm', hidden: true }));
  expect(
    await screen.findByText('Indexing queued. Start another scan to verify the current state.'),
  ).toBeInTheDocument();
  expect(screen.getByRole('button', { name: 'Delete content' })).toBeDisabled();
  expect(api.resolve).toHaveBeenCalledWith(700, 'index', 'postgres', 'content', 'scan-1');
});

it.each([
  { items: [], scanned: 200, nextAfter: 200, complete: false },
  { ...completed, pending: undefined },
  { ...completed, scanned: undefined },
  { ...completed, found: undefined },
])(
  'rejects incompatible scan responses without rendering invalid progress: %j',
  async (response) => {
    api.latest.mockResolvedValue(response);
    renderPage();
    await waitFor(() =>
      expect(screen.getByRole('status')).toHaveTextContent('Scan status is incompatible'),
    );
    expect(screen.getByRole('button', { name: 'Start new scan' })).toBeDisabled();
    expect(api.results).not.toHaveBeenCalled();
  },
);

it('renders valid zero counters without treating them as missing', async () => {
  api.latest.mockResolvedValue({ ...completed, scanned: 0, pending: 0, found: 0 });
  api.results.mockResolvedValue({ items: [], nextAfter: 0, hasMore: false });
  renderPage();
  await waitFor(() =>
    expect(screen.getByRole('status')).toHaveTextContent(
      '0 records checked; 0 awaiting verification; 0 confirmed orphans.',
    ),
  );
  expect(screen.getByText('No orphaned content found for this comparison.')).toBeInTheDocument();
});

it('rejects reversed posted dates before starting a scan', async () => {
  const user = userEvent.setup();
  renderPage();
  await waitFor(() => expect(screen.getByRole('button', { name: 'Start new scan' })).toBeEnabled());
  fireEvent.change(screen.getByLabelText('Start posted date (optional)'), {
    target: { value: '2026-10-25' },
  });
  fireEvent.change(screen.getByLabelText('End posted date (optional)'), {
    target: { value: '2026-10-01' },
  });
  await user.click(screen.getByRole('button', { name: 'Start new scan' }));
  expect(screen.getByRole('status')).toHaveTextContent(
    'End posted date must be on or after start posted date.',
  );
  expect(api.start).not.toHaveBeenCalled();
});

it.each([
  ['', '', undefined, undefined],
  ['2026-10-01', '', '2026-10-01', undefined],
  ['', '2026-10-25', undefined, '2026-10-25'],
])(
  'supports optional posted date bounds %s through %s',
  async (start, end, expectedStart, expectedEnd) => {
    const user = userEvent.setup();
    api.start.mockResolvedValue({
      ...completed,
      startPostedOn: expectedStart,
      endPostedOn: expectedEnd,
    });
    renderPage();
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Start new scan' })).toBeEnabled(),
    );
    fireEvent.change(screen.getByLabelText('Start posted date (optional)'), {
      target: { value: start },
    });
    fireEvent.change(screen.getByLabelText('End posted date (optional)'), {
      target: { value: end },
    });
    await user.click(screen.getByRole('button', { name: 'Start new scan' }));
    expect(api.start).toHaveBeenCalledWith('elasticsearch', 'content', expectedStart, expectedEnd);
    expect(screen.queryByLabelText('Start content ID (optional)')).not.toBeInTheDocument();
  },
);
