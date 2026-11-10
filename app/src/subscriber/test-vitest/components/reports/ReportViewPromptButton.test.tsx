import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest';

import { ReportViewPromptButton } from '../../../features/my-reports/edit/view/ReportViewPromptButton';
import { TestWrapper } from '../../utils';

const state = vi.hoisted(() => ({
  describe: vi.fn(),
  values: { sections: [] as any[] },
}));

vi.mock('store/hooks', () => ({
  useReportAI: () => ({ describeInstanceSection: state.describe }),
}));
vi.mock('../../../features/my-reports/edit/ReportEditContext', () => ({
  useReportEditContext: () => ({ values: state.values }),
}));

const aiSection = (id: number, label: string) => ({
  id,
  name: `ai-${id}`,
  sectionType: 'AI',
  isEnabled: true,
  settings: { label },
});

const prompt = (sectionId: number, label: string) => ({
  sectionId,
  sectionLabel: label,
  llmName: 'Alpha',
  isPlanned: true,
  exchanges: [
    {
      stage: 'Final',
      kind: 'Final',
      sequence: 1,
      messages: [{ role: 'user', content: `Prompt for ${label}` }],
      durationMs: 0,
      jsonMode: false,
      isTruncated: false,
      estimatedTokens: 5,
    },
  ],
});

describe('ReportViewPromptButton', () => {
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
    state.describe.mockReset();
  });

  it('is absent without an enabled AI section or an instance', () => {
    state.values = { sections: [{ id: 1, sectionType: 'Content', isEnabled: true, settings: {} }] };
    const { rerender } = render(
      <TestWrapper>
        <ReportViewPromptButton instanceId={7} />
      </TestWrapper>,
    );
    expect(screen.queryByRole('button', { name: 'View AI prompts' })).not.toBeInTheDocument();
    state.values = { sections: [aiSection(9, 'Summary')] };
    rerender(
      <TestWrapper>
        <ReportViewPromptButton instanceId={undefined} />
      </TestWrapper>,
    );
    expect(screen.queryByRole('button', { name: 'View AI prompts' })).not.toBeInTheDocument();
  });

  it('describes the chosen section for the previewed instance', async () => {
    state.values = { sections: [aiSection(9, 'Summary'), aiSection(10, 'Trends')] };
    state.describe.mockImplementation((_: number, id: number) =>
      Promise.resolve(prompt(id, id === 9 ? 'Summary' : 'Trends')),
    );
    render(
      <TestWrapper>
        <ReportViewPromptButton instanceId={7} />
      </TestWrapper>,
    );
    fireEvent.click(screen.getByRole('button', { name: 'View AI prompts' }));
    await waitFor(() => expect(state.describe).toHaveBeenCalledWith(7, 9));
    const dialog = await screen.findByRole('dialog', { name: 'AI section prompt: Summary' });
    expect(within(dialog).getByText('Prompt for Summary')).toBeInTheDocument();
    expect(dialog).toHaveTextContent('Planned prompt');
    fireEvent.change(within(dialog).getByRole('combobox', { name: 'AI section' }), {
      target: { value: '10' },
    });
    await waitFor(() => expect(state.describe).toHaveBeenCalledWith(7, 10));
    expect(await screen.findByText('Prompt for Trends')).toBeInTheDocument();
  });
});
