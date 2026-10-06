import { act, render, screen } from '@testing-library/react';
import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { ContentAnalysis } from '../../../features/content/view-content/ContentAnalysis';

const getAnalysis = vi.hoisted(() => vi.fn());
vi.mock('store/hooks/subscriber/useContentAnalysis', () => ({
  useContentAnalysis: () => getAnalysis,
}));
vi.mock('tno-core', () => ({
  Col: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Show: ({ visible, children }: { visible: boolean; children: React.ReactNode }) =>
    visible ? <>{children}</> : null,
}));

const analysis = {
  analyzedOn: '2026-10-06T12:00:00Z',
  model: 'Test model',
  summary: 'Story summary',
  keyFacts: [{ statement: 'An inferred fact', isInferred: true }],
  topics: [{ label: 'Housing' }],
  entities: [{ name: 'Person', roles: ['Minister'], isAmbiguous: true }],
  places: [{ name: 'Victoria', role: 'location' }],
  events: [{ actor: 'Person', action: 'announced funding' }],
  quotes: [{ statement: 'Excluded quote' }],
};

describe('ContentAnalysis', () => {
  beforeEach(() => {
    getAnalysis.mockReset();
  });

  it('shows the subscriber sections and inference labels without quotes or editorial fields', async () => {
    getAnalysis.mockResolvedValue(analysis);
    render(<ContentAnalysis contentId={42} />);
    expect(await screen.findByText('Story summary')).toBeInTheDocument();
    for (const name of [
      'Summary',
      'Key facts',
      'Topics',
      'People and organizations',
      'Places',
      'Reported events',
    ]) {
      expect(screen.getByRole('heading', { name })).toBeInTheDocument();
    }
    expect(screen.getByText('(inferred)')).toBeInTheDocument();
    expect(screen.getByText('(ambiguous)')).toBeInTheDocument();
    expect(screen.queryByText('Quotes found')).not.toBeInTheDocument();
    expect(screen.queryByText('Excluded quote')).not.toBeInTheDocument();
    expect(screen.queryByText('Fields')).not.toBeInTheDocument();
    expect(screen.queryByText('Analyze again')).not.toBeInTheDocument();
    expect(getAnalysis).toHaveBeenCalledWith(42);
  });

  it('distinguishes loading, missing analysis, and a failed request', async () => {
    getAnalysis.mockResolvedValue(null);
    const { unmount } = render(<ContentAnalysis contentId={42} />);
    expect(screen.getByText('Loading analysis...')).toBeInTheDocument();
    expect(screen.queryByText('This story has not been analyzed.')).not.toBeInTheDocument();
    expect(await screen.findByText('This story has not been analyzed.')).toBeInTheDocument();
    unmount();
    getAnalysis.mockRejectedValue(new Error('Network failure'));
    render(<ContentAnalysis contentId={43} />);
    expect(await screen.findByText(/Unable to load analysis/)).toBeInTheDocument();
    expect(screen.queryByText('This story has not been analyzed.')).not.toBeInTheDocument();
  });

  it('ignores a late response for a previous story', async () => {
    let resolvePrevious!: (value: typeof analysis) => void;
    getAnalysis.mockReturnValueOnce(
      new Promise((resolve) => {
        resolvePrevious = resolve;
      }),
    );
    const { rerender } = render(<ContentAnalysis contentId={42} />);
    getAnalysis.mockResolvedValueOnce({ ...analysis, summary: 'Current story' });
    rerender(<ContentAnalysis contentId={43} />);
    expect(await screen.findByText('Current story')).toBeInTheDocument();
    await act(async () => resolvePrevious(analysis));
    expect(screen.queryByText('Story summary')).not.toBeInTheDocument();
  });
});
