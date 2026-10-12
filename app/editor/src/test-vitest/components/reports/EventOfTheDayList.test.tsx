import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import EventOfTheDayList from 'features/admin/event-of-the-day/EventOfTheDayList';
import moment from 'moment-timezone';
import React from 'react';
import { MemoryRouter } from 'react-router-dom';
import { vi } from 'vitest';

const mocks = vi.hoisted(() => ({
  find: vi.fn(),
  preview: vi.fn(),
  save: vi.fn(),
  view: vi.fn(),
  topics: vi.fn(),
  update: vi.fn(),
  reset: vi.fn(),
  folder: vi.fn(),
  addFolder: vi.fn(),
  publish: vi.fn(),
}));
const history = {
  find: mocks.find,
  preview: mocks.preview,
  save: mocks.save,
  addToFolder: mocks.addFolder,
};
const settings = [
  { name: 'folder', value: '1' },
  { name: 'report', value: '15' },
];
vi.mock('store/hooks/editor/useEventOfDayReports', () => ({
  useEventOfDayReports: () => ({ ...history, find: (date: string) => mocks.find(date) }),
}));
vi.mock('store/hooks', () => ({
  useLookup: () => [{ isReady: true, settings }],
  useReports: () => [{ publishReport: mocks.publish }],
  useReportInstances: () => [{ viewReportInstance: mocks.view }],
  useFolders: () => ({ getContentInFolder: (...args: any[]) => mocks.folder(...args) }),
  useContent: () => [
    null,
    { updateContentTopics: mocks.update, resetContentTopicScore: mocks.reset },
  ],
}));
vi.mock('store/hooks/admin', () => ({
  useTopics: () => [
    null,
    { findAllTopics: () => mocks.topics(), updateTopic: vi.fn(), addTopic: vi.fn() },
  ],
}));
vi.mock('features/content', () => ({ getPreviewReportRoute: () => '/preview' }));
vi.mock('features/admin/topics', () => ({ TopicFormSmall: () => null }));
vi.mock('features/admin/event-of-the-day/hooks', () => ({ useColumns: () => [] }));
vi.mock('components/formpage', () => ({
  FormPage: ({ children }: React.PropsWithChildren) => <div>{children}</div>,
}));
vi.mock('react-toastify', () => ({ toast: { success: vi.fn(), warn: vi.fn(), error: vi.fn() } }));
vi.mock('tno-core', () => ({
  Settings: { EventOfTheDayFolder: 'folder', EventOfTheDayReport: 'report' },
  ButtonVariant: { secondary: 'secondary' },
  Button: ({ children, variant, ...props }: any) => <button {...props}>{children}</button>,
  Row: ({ children, ...props }: React.HTMLAttributes<HTMLDivElement>) => (
    <div {...props}>{children}</div>
  ),
  Show: ({ visible, children }: React.PropsWithChildren<{ visible: boolean }>) =>
    visible ? <>{children}</> : null,
  Modal: () => null,
  useModal: () => ({ toggle: vi.fn(), isShowing: false }),
  Select: ({ options, onChange, label, isDisabled }: any) => (
    <label>
      {label}
      <select
        disabled={isDisabled}
        value=""
        onChange={(e) => onChange(options.find((o: any) => String(o.value) === e.target.value))}
      >
        <option value="" />
        {options.map((o: any) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </select>
    </label>
  ),
  FlexboxTable: ({ columns, data }: any) => (
    <div>
      {columns[0].label}
      {data.map((item: any) => (
        <div key={item.contentId}>
          {item.content.headline}
          {columns[0].cell({ original: item })}
        </div>
      ))}
    </div>
  ),
}));

const instance = {
  id: 65177,
  publishedOn: '2026-10-11T17:00:00Z',
  version: 4,
  subject: 'Saved',
  contentIds: [11],
};
const request = (
  changedContentIds: number[] = [],
  addContentIds: number[] = [],
  recollect = false,
) => ({
  changedContentIds,
  addContentIds,
  recollect,
});
const story = (id: number) => ({
  contentId: id,
  sortOrder: id,
  content: {
    headline: `Story ${id}`,
    topics: [{ id: 1, name: 'Issue', score: 10, isSystem: false }],
  },
});
const draft = {
  instanceId: 65177,
  reportId: 15,
  version: 4,
  subject: 'New',
  body: '<p>Updated chart</p>',
  content: [],
};

beforeEach(() => {
  vi.clearAllMocks();
  mocks.find.mockResolvedValue({ instances: [instance], content: [story(11), story(12)] });
  mocks.topics.mockResolvedValue([{ id: 99, name: 'Not Applicable', isSystem: true }]);
  mocks.preview.mockResolvedValue(draft);
  mocks.save.mockResolvedValue({ ...instance, version: 5 });
  mocks.view.mockResolvedValue({ body: '<p>Original chart</p>' });
  mocks.update.mockResolvedValue([{ id: 99, name: 'Not Applicable', isSystem: true, score: 0 }]);
});

const open = (date = '2026-10-11') =>
  render(
    <MemoryRouter initialEntries={[`/reports/event-of-the-day?date=${date}`]}>
      <EventOfTheDayList />
    </MemoryRouter>,
  );

it('loads once despite recreated controllers and reloads only on explicit Refresh or date changes', async () => {
  open();
  await screen.findByText('Story 11');
  await userEvent.click(screen.getByRole('checkbox', { name: 'Select story 11' }));
  await userEvent.click(screen.getByRole('button', { name: 'Clear selection' }));
  expect(mocks.topics).toHaveBeenCalledTimes(1);
  expect(mocks.find).toHaveBeenCalledTimes(1);

  await userEvent.click(screen.getByRole('button', { name: 'Refresh' }));
  await screen.findByText('Story 11');
  expect(mocks.topics).toHaveBeenCalledTimes(2);
  expect(mocks.find).toHaveBeenCalledTimes(2);

  await userEvent.click(screen.getByRole('button', { name: 'Previous day' }));
  await screen.findByText('Story 11');
  expect(mocks.topics).toHaveBeenCalledTimes(2);
  expect(mocks.find).toHaveBeenCalledTimes(3);
  expect(mocks.find).toHaveBeenLastCalledWith('2026-10-10');
});

it('loads historical source stories and saves only after explicitly reviewing a regenerated preview', async () => {
  open();
  await screen.findByText('Story 11');
  expect(mocks.find).toHaveBeenCalledWith('2026-10-11');
  expect(mocks.folder).not.toHaveBeenCalled();
  expect(screen.queryByRole('button', { name: 'Send' })).not.toBeInTheDocument();
  expect(screen.queryByRole('button', { name: 'Save report' })).not.toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Regenerate and preview' }));
  await screen.findByText('Regenerated preview — not saved');
  expect(mocks.preview).toHaveBeenCalledWith(65177, request(), '2026-10-11');
  expect(mocks.save).not.toHaveBeenCalled();
  expect(screen.getByTitle('Event of the Day report preview')).toHaveAttribute(
    'srcdoc',
    draft.body,
  );
  await userEvent.click(screen.getByRole('button', { name: 'Save report' }));
  await waitFor(() => expect(mocks.save).toHaveBeenCalledWith(draft, '2026-10-11'));
  expect(mocks.publish).not.toHaveBeenCalled();
  await waitFor(() =>
    expect(screen.queryByRole('button', { name: 'Save report' })).not.toBeInTheDocument(),
  );
});

it('invalidates a preview after source corrections and passes the changed IDs to regeneration', async () => {
  open();
  await screen.findByText('Story 11');
  await userEvent.click(screen.getByRole('button', { name: 'Regenerate and preview' }));
  await screen.findByRole('button', { name: 'Save report' });
  await userEvent.click(screen.getByRole('checkbox', { name: 'Select story 12' }));
  await userEvent.click(screen.getByRole('button', { name: 'Mark selected Not Applicable' }));
  await waitFor(() =>
    expect(mocks.update).toHaveBeenCalledWith(12, [expect.objectContaining({ id: 99, score: 0 })]),
  );
  expect(screen.queryByRole('button', { name: 'Save report' })).not.toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Regenerate and preview' }));
  await waitFor(() =>
    expect(mocks.preview).toHaveBeenLastCalledWith(65177, request([12]), '2026-10-11'),
  );
});

it('clears the previous preview when changing dates and handles days without a saved report', async () => {
  open();
  await screen.findByText('Story 11');
  await userEvent.click(screen.getByRole('button', { name: 'View saved report' }));
  await screen.findByTitle('Event of the Day report preview');
  expect(screen.queryByRole('button', { name: 'Save report' })).not.toBeInTheDocument();
  mocks.find.mockResolvedValue({ instances: [], content: [] });
  await userEvent.click(screen.getByRole('button', { name: 'Previous day' }));
  await screen.findByText(/No report was generated on this date/);
  expect(mocks.find).toHaveBeenLastCalledWith('2026-10-10');
  expect(screen.queryByTitle('Event of the Day report preview')).not.toBeInTheDocument();
  expect(screen.queryByRole('button', { name: 'Regenerate and preview' })).not.toBeInTheDocument();
  expect(screen.getByRole('button', { name: 'Generate report for this date' })).toBeEnabled();
});

it('generates a report for a past day without one and saves it as a new instance', async () => {
  mocks.find.mockResolvedValue({ instances: [], content: [story(11)] });
  const generated = { ...draft, instanceId: 0, version: 0, publishedOn: '2026-10-11T17:00:00Z' };
  mocks.preview.mockResolvedValue(generated);
  mocks.save.mockResolvedValue({ ...instance, id: 70001, version: 0, contentIds: [11] });
  open();
  await screen.findByText(/No report was generated on this date/);
  expect(screen.queryByRole('button', { name: 'Send' })).not.toBeInTheDocument();
  // Stories can be added before the first save; the generated report has none yet.
  await userEvent.selectOptions(screen.getByLabelText('Add a story to the report'), '11');
  await userEvent.click(screen.getByRole('button', { name: 'Generate report for this date' }));
  await screen.findByText('Generated preview — not saved');
  expect(mocks.preview).toHaveBeenCalledWith(0, request([], [11]), '2026-10-11');
  await userEvent.click(screen.getByRole('button', { name: 'Save report' }));
  await waitFor(() => expect(mocks.save).toHaveBeenCalledWith(generated, '2026-10-11'));
  await screen.findByText('Saved report');
  expect(screen.getByRole('combobox', { name: 'Report instance' })).toHaveValue('70001');
  expect(
    screen.queryByRole('button', { name: 'Generate report for this date' }),
  ).not.toBeInTheDocument();
  expect(screen.getByRole('button', { name: 'Regenerate and preview' })).toBeEnabled();
});

it('does not offer to save a failed regeneration', async () => {
  mocks.preview.mockRejectedValue(new Error('Indexing in progress'));
  open();
  await screen.findByText('Story 11');
  await userEvent.click(screen.getByRole('button', { name: 'Regenerate and preview' }));
  await waitFor(() =>
    expect(screen.getByRole('button', { name: 'Regenerate and preview' })).toBeEnabled(),
  );
  expect(screen.queryByRole('button', { name: 'Save report' })).not.toBeInTheDocument();
  expect(mocks.save).not.toHaveBeenCalled();
});

it('does not overwrite the current date with a late response for the previous date', async () => {
  let finish: (value: any) => void = () => {};
  mocks.find.mockReturnValueOnce(
    new Promise((resolve) => {
      finish = resolve;
    }),
  );
  open();
  // Simulate URL-driven navigation even while the date control is disabled.
  fireEvent.change(screen.getByLabelText('Publication date (BC)'), {
    target: { value: '2026-10-10' },
  });
  await screen.findByText('Story 11');
  finish({ instances: [], content: [story(999)] });
  await waitFor(() => expect(screen.queryByText('Story 999')).not.toBeInTheDocument());
});

it('adds a story with a topic from the date to the report, and re-collects on request', async () => {
  open();
  await screen.findByText('Story 11');
  // Story 11 is already in the report, so only story 12 can be added.
  const picker = screen.getByLabelText('Add a story to the report');
  expect(screen.getByRole('option', { name: 'Story 12' })).toBeInTheDocument();
  expect(screen.queryByRole('option', { name: 'Story 11' })).not.toBeInTheDocument();
  await userEvent.selectOptions(picker, '12');
  expect(screen.getByRole('list', { name: 'Stories to add' })).toHaveTextContent('Story 12');
  expect(screen.queryByRole('option', { name: 'Story 12' })).not.toBeInTheDocument();

  await userEvent.click(screen.getByRole('button', { name: 'Regenerate and preview' }));
  await waitFor(() =>
    expect(mocks.preview).toHaveBeenLastCalledWith(65177, request([], [12]), '2026-10-11'),
  );
  await screen.findByText('Regenerated preview — not saved');

  await userEvent.click(screen.getByRole('button', { name: 'Remove story 12' }));
  expect(screen.queryByText('Regenerated preview — not saved')).not.toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Re-collect stories and preview' }));
  await waitFor(() =>
    expect(mocks.preview).toHaveBeenLastCalledWith(65177, request([], [], true), '2026-10-11'),
  );
  await screen.findByText('Re-collected preview — not saved');
  mocks.save.mockResolvedValue({ ...instance, version: 5, contentIds: [11, 12] });
  await userEvent.click(screen.getByRole('button', { name: 'Save report' }));
  await screen.findByText('Saved report');
  expect(screen.queryByRole('option', { name: 'Story 12' })).not.toBeInTheDocument();
});

it("adds one of today's stories with a topic straight into the live folder", async () => {
  const today = moment.tz('America/Vancouver').format('YYYY-MM-DD');
  mocks.folder.mockResolvedValue([story(11)]);
  mocks.find.mockResolvedValue({ instances: [], content: [story(11), story(12)] });
  mocks.addFolder.mockResolvedValue(undefined);
  open(today);
  await screen.findByText('Story 11');
  expect(screen.getByRole('button', { name: 'Send' })).toBeInTheDocument();
  expect(screen.queryByRole('checkbox', { name: 'Select story 12' })).not.toBeInTheDocument();
  const picker = screen.getByLabelText('Add a story to the folder');
  expect(screen.getByRole('option', { name: 'Story 12' })).toBeInTheDocument();
  expect(screen.queryByRole('option', { name: 'Story 11' })).not.toBeInTheDocument();
  mocks.folder.mockResolvedValue([story(11), story(12)]);
  await userEvent.selectOptions(picker, '12');
  await waitFor(() => expect(mocks.addFolder).toHaveBeenCalledWith(12));
  await screen.findByRole('checkbox', { name: 'Select story 12' });
  expect(mocks.folder).toHaveBeenLastCalledWith(1, true);
  expect(screen.queryByRole('option', { name: 'Story 12' })).not.toBeInTheDocument();
  expect(mocks.preview).not.toHaveBeenCalled();
});
