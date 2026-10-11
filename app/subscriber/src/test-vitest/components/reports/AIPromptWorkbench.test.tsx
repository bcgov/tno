import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest';

import { ReportEditContext } from '../../../features/my-reports/edit/ReportEditContext';
import {
  AI_TEST_POLL_MS,
  AIPromptWorkbench,
} from '../../../features/my-reports/edit/settings/template/AIPromptWorkbench';
import { TestWrapper } from '../../utils';

const state = vi.hoisted(() => ({
  api: {
    describeSection: vi.fn(),
    describeInstanceSection: vi.fn(),
    testSection: vi.fn(),
    getTest: vi.fn(),
    chat: vi.fn(),
  },
  hub: undefined as undefined | ((message: any) => Promise<void>),
}));

vi.mock('store/hooks', () => ({
  useReportAI: () => state.api,
  useApiHub: () => ({
    useHubEffect: (_: string, callback: (message: any) => Promise<void>) => {
      state.hub = callback;
    },
  }),
}));
vi.mock('react-toastify', () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const pending = (testId = 't1') => ({
  testId,
  reportId: 1,
  reportInstanceId: 7,
  sectionId: 9,
  status: 'Pending',
  requestCount: 0,
  promptTokens: 0,
  completionTokens: 0,
  durationMs: 0,
  storyCount: 0,
  reductionDepth: 0,
  transcript: [],
  createdOn: new Date().toISOString(),
});

const completed = (testId = 't1') => ({
  ...pending(testId),
  status: 'Completed',
  output: '<p>Done output</p>',
  requestCount: 2,
  promptTokens: 120,
  completionTokens: 40,
  durationMs: 2500,
  storyCount: 3,
  transcript: [
    {
      stage: 'Final',
      kind: 'Final',
      sequence: 1,
      messages: [{ role: 'user', content: 'Final request text' }],
      response: '<p>Done output</p>',
      durationMs: 2500,
      jsonMode: false,
      isTruncated: false,
      estimatedTokens: 100,
    },
  ],
});

const renderWorkbench = (values: any, onClose = vi.fn(), setFieldValue = vi.fn()) => {
  const utils = render(
    <TestWrapper>
      <ReportEditContext.Provider value={{ values, setFieldValue, isSubmitting: false } as any}>
        <AIPromptWorkbench index={0} open onClose={onClose} />
      </ReportEditContext.Provider>
    </TestWrapper>,
  );
  return { ...utils, onClose, setFieldValue };
};

const savedReport = () => ({
  id: 1,
  instances: [{ id: 7 }],
  sections: [
    {
      id: 9,
      name: 'ai',
      sectionType: 'AI',
      isEnabled: true,
      settings: { label: 'Summary', llmId: 1, userPrompt: '<p>Hello prompt</p>' },
    },
  ],
});

describe('AIPromptWorkbench', () => {
  const originalShow = HTMLDialogElement.prototype.showModal;
  const originalClose = HTMLDialogElement.prototype.close;
  beforeAll(() => {
    HTMLDialogElement.prototype.showModal = function () {
      this.open = true;
    };
    HTMLDialogElement.prototype.close = function () {
      this.open = false;
    };
  });
  afterAll(() => {
    HTMLDialogElement.prototype.showModal = originalShow;
    HTMLDialogElement.prototype.close = originalClose;
  });
  beforeEach(() => {
    Object.values(state.api).forEach((fn) => fn.mockReset());
    state.hub = undefined;
  });
  afterEach(() => {
    vi.useRealTimers();
  });

  it('disables Generate until the report is saved and generated', () => {
    const { unmount } = renderWorkbench({
      id: 0,
      instances: [],
      sections: [{ id: 0, name: 'ai', settings: { label: 'Summary', userPrompt: 'Hi' } }],
    });
    const generate = screen.getByRole('button', { name: 'Generate' });
    expect(generate).toBeDisabled();
    expect(generate).toHaveAttribute('title', 'Save the report to test this section');
    unmount();

    renderWorkbench({ ...savedReport(), instances: [] });
    expect(screen.getByRole('button', { name: 'Generate' })).toHaveAttribute(
      'title',
      'Generate the report to test this section',
    );
  });

  it('runs the draft, follows its own hub notice, and shows the output', async () => {
    state.api.testSection.mockResolvedValue(pending());
    state.api.getTest.mockResolvedValue(completed());
    renderWorkbench(savedReport());

    fireEvent.click(screen.getByRole('button', { name: 'Generate' }));
    await waitFor(() =>
      expect(state.api.testSection).toHaveBeenCalledWith(
        1,
        9,
        expect.objectContaining({
          label: 'Summary',
          userPrompt: expect.stringContaining('Hello prompt'),
        }),
      ),
    );
    expect(await screen.findByRole('status')).toHaveTextContent('Generating');

    // Another user's run is not ours.
    await act(async () => {
      await state.hub!({ id: 7, reportId: 1, message: 'ai-test:other' });
    });
    expect(state.api.getTest).not.toHaveBeenCalled();

    await act(async () => {
      await state.hub!({ id: 7, reportId: 1, message: 'ai-test:t1' });
    });
    await waitFor(() => expect(state.api.getTest).toHaveBeenCalledWith(1, 9, 't1'));
    expect(await screen.findByText('Done output')).toBeInTheDocument();
    expect(screen.getByRole('status')).toHaveTextContent('Completed');
    expect(screen.getByText(/2 requests · 3 stories · 120 prompt tokens/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('tab', { name: 'Transcript' }));
    expect(await screen.findByText('Final request text')).toBeInTheDocument();
    expect(screen.getByText(/Actual prompt/)).toBeInTheDocument();
    expect(state.api.describeSection).not.toHaveBeenCalled();
  });

  it('polls a pending run until it completes', async () => {
    vi.useFakeTimers();
    state.api.testSection.mockResolvedValue(pending());
    state.api.getTest.mockResolvedValueOnce(pending()).mockResolvedValue(completed());
    renderWorkbench(savedReport());

    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Generate' }));
    });
    await act(async () => {
      await vi.advanceTimersByTimeAsync(AI_TEST_POLL_MS);
    });
    expect(state.api.getTest).toHaveBeenCalledTimes(1);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(AI_TEST_POLL_MS);
    });
    expect(state.api.getTest).toHaveBeenCalledTimes(2);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(AI_TEST_POLL_MS * 3);
    });
    expect(state.api.getTest).toHaveBeenCalledTimes(2);
    expect(screen.getByText('Done output')).toBeInTheDocument();
  });

  it('shows the planned prompt for the draft before any run', async () => {
    state.api.describeSection.mockResolvedValue({
      sectionId: 9,
      sectionLabel: 'Summary',
      isPlanned: true,
      exchanges: [
        {
          stage: 'Map batch 1/1',
          kind: 'Map',
          sequence: 1,
          messages: [{ role: 'user', content: '[S1] Story\nsection: news' }],
          durationMs: 0,
          jsonMode: true,
          isTruncated: false,
          estimatedTokens: 9,
        },
      ],
    });
    renderWorkbench(savedReport());

    fireEvent.click(screen.getByRole('button', { name: 'View prompt' }));
    await waitFor(() =>
      expect(state.api.describeSection).toHaveBeenCalledWith(
        1,
        9,
        expect.objectContaining({ userPrompt: expect.stringContaining('Hello prompt') }),
      ),
    );
    expect(await screen.findByText('Map batch 1/1')).toBeInTheDocument();
    expect(screen.getByText(/Planned prompt/)).toBeInTheDocument();
  });

  it('chats about the prompt and applies a suggested prompt to the section', async () => {
    state.api.chat
      .mockResolvedValueOnce({
        prompt: 'composed',
        answer: 'Try a shorter prompt.',
        suggestedPrompt: '<p>New shorter prompt</p>',
        messages: [
          { role: 'system', content: 'sys' },
          { role: 'user', content: 'composed' },
          { role: 'assistant', content: 'Try a shorter prompt.' },
        ],
      })
      .mockResolvedValueOnce({
        prompt: '',
        answer: 'Yes, that works.',
        messages: [{ role: 'assistant', content: 'Yes, that works.' }],
      });
    const { onClose, setFieldValue } = renderWorkbench(savedReport());

    fireEvent.click(screen.getByRole('tab', { name: 'Chat' }));
    fireEvent.change(screen.getByRole('textbox', { name: 'Question' }), {
      target: { value: 'Shorter?' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Send' }));
    await waitFor(() =>
      expect(state.api.chat).toHaveBeenCalledWith(
        1,
        9,
        expect.objectContaining({ question: 'Shorter?', messages: [], testId: undefined }),
      ),
    );
    expect(await screen.findByText('Try a shorter prompt.')).toBeInTheDocument();
    expect(screen.getByText('Shorter?')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Use this prompt' }));
    const dialog = screen.getByRole('dialog', { name: 'Prompt workbench: Summary' });
    await waitFor(() =>
      expect(
        within(dialog).getByText('New shorter prompt', { selector: '.ql-editor p' }),
      ).toBeInTheDocument(),
    );

    fireEvent.change(screen.getByRole('textbox', { name: 'Question' }), {
      target: { value: 'Good?' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Send' }));
    await waitFor(() =>
      expect(state.api.chat).toHaveBeenLastCalledWith(
        1,
        9,
        expect.objectContaining({
          question: 'Good?',
          messages: expect.arrayContaining([
            { role: 'assistant', content: 'Try a shorter prompt.' },
          ]),
        }),
      ),
    );
    expect(await screen.findByText('Yes, that works.')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Apply to section' }));
    expect(setFieldValue).toHaveBeenCalledWith(
      'sections.0.settings.userPrompt',
      expect.stringContaining('New shorter prompt'),
    );
    expect(onClose).toHaveBeenCalled();
  });
});
