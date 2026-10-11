import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { describe, expect, it, vi } from 'vitest';

import {
  AIPromptTranscript,
  formatTranscript,
} from '../../../features/my-reports/components/AIPromptTranscript';

vi.mock('react-toastify', () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const exchanges = [
  {
    stage: 'Final',
    kind: 'Final',
    sequence: 3,
    messages: [
      { role: 'system', content: 'Write the section.' },
      { role: 'user', content: 'Findings and prompt' },
    ],
    response: '<p>Out</p>',
    promptTokens: 300,
    completionTokens: 50,
    durationMs: 1200,
    jsonMode: false,
    isTruncated: false,
    estimatedTokens: 290,
  },
  {
    stage: 'Map batch 1/2',
    kind: 'Map',
    sequence: 1,
    messages: [
      { role: 'system', content: 'Read stories.' },
      { role: 'user', content: '[S1] Headline\nsection: news' },
    ],
    response: '{"findings":[]}',
    promptTokens: 100,
    completionTokens: 20,
    durationMs: 800,
    jsonMode: true,
    isTruncated: false,
    estimatedTokens: 98,
  },
  {
    stage: 'Map batch 2/2',
    kind: 'Map',
    sequence: 2,
    messages: [
      { role: 'system', content: 'Read stories.' },
      { role: 'user', content: '[S2] Other' },
    ],
    error: 'context length exceeded',
    durationMs: 100,
    jsonMode: true,
    isTruncated: false,
    estimatedTokens: 90,
  },
];

describe('AIPromptTranscript', () => {
  it('groups messages by stage in send order and shows responses and errors', () => {
    const { container } = render(<AIPromptTranscript exchanges={exchanges} />);
    const stages = Array.from(
      container.querySelectorAll('details > summary > span:first-child'),
    ).map((e) => e.textContent);
    expect(stages).toEqual(['Map batch 1/2', 'Map batch 2/2', 'Final']);
    expect(
      screen.getByText('[S1] Headline section: news', {
        normalizer: (t) => t.replace(/\s+/g, ' '),
      }),
    ).toBeInTheDocument();
    expect(screen.getAllByText('response')).toHaveLength(2);
    expect(screen.getAllByText('context length exceeded').length).toBeGreaterThan(0);
    expect(screen.getByText(/Actual prompt/)).toBeInTheDocument();
  });

  it('hides responses and explains itself in planned mode', () => {
    const planned = exchanges.map((e) => ({ ...e, response: undefined, error: undefined }));
    render(<AIPromptTranscript exchanges={planned} isPlanned />);
    expect(screen.getByText(/Planned prompt/)).toBeInTheDocument();
    expect(screen.queryByText('response')).not.toBeInTheDocument();
    expect(screen.getByText('~98 tokens · JSON')).toBeInTheDocument();
  });

  it('copies the whole transcript as text', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.assign(navigator, { clipboard: { writeText } });
    render(<AIPromptTranscript exchanges={exchanges} />);
    fireEvent.click(screen.getByRole('button', { name: 'Copy all' }));
    await waitFor(() => expect(writeText).toHaveBeenCalledTimes(1));
    const text = writeText.mock.calls[0][0] as string;
    expect(text).toBe(formatTranscript(exchanges));
    expect(text.indexOf('=== Map batch 1/2 ===')).toBeLessThan(text.indexOf('=== Final ==='));
    expect(text).toContain('--- user ---\n[S1] Headline\nsection: news');
    expect(text).toContain('--- response ---\n<p>Out</p>');
    expect(text).toContain('--- error ---\ncontext length exceeded');
  });

  it('shows the error or the loading state instead of an empty list', () => {
    const { rerender } = render(<AIPromptTranscript exchanges={[]} error="No model" />);
    expect(screen.getByRole('alert')).toHaveTextContent('No model');
    rerender(<AIPromptTranscript exchanges={[]} isLoading />);
    expect(screen.getByText('Loading the prompt…')).toBeInTheDocument();
    rerender(<AIPromptTranscript exchanges={[]} />);
    expect(screen.getByText('Nothing has been sent to the model yet.')).toBeInTheDocument();
  });
});
