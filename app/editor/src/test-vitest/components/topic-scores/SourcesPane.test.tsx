import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RemoveScoringSource } from 'features/admin/topic-score-rules/RemoveScoringSource';
import { SourcesPane } from 'features/admin/topic-score-rules/SourcesPane';
import React from 'react';
import { TestWrapper } from 'test/utils';
import { vi } from 'vitest';

const source = {
  id: 1,
  name: 'Daily',
  code: 'DLY',
  useInTopics: true,
  seriesUseInTopics: false,
  ruleCount: 3,
};

it('shows Default Score without a rules count and removes without selecting the row', async () => {
  const onSelect = vi.fn();
  const onRemove = vi.fn();
  render(
    <TestWrapper>
      <SourcesPane
        sources={[source]}
        onSelect={onSelect}
        onRemove={onRemove}
        onAdd={vi.fn()}
        onDefaultScoreChange={vi.fn()}
      />
    </TestWrapper>,
  );
  expect(screen.getByText('Default Score')).toBeInTheDocument();
  expect(screen.queryByText('Rules')).not.toBeInTheDocument();
  expect(screen.queryByText('3')).not.toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Remove Daily from topic scoring' }));
  expect(onRemove).toHaveBeenCalledWith(source);
  expect(onSelect).not.toHaveBeenCalled();
});

it('flags a source that neither it nor its series uses topics', () => {
  render(
    <TestWrapper>
      <SourcesPane
        sources={[
          source,
          { ...source, id: 2, name: 'Weekly', code: 'WKY', useInTopics: false },
          {
            ...source,
            id: 3,
            name: 'Shows',
            code: 'SHW',
            useInTopics: false,
            seriesUseInTopics: true,
          },
        ]}
        onSelect={vi.fn()}
        onRemove={vi.fn()}
        onAdd={vi.fn()}
        onDefaultScoreChange={vi.fn()}
      />
    </TestWrapper>,
  );
  expect(screen.getAllByText('(not scored)')).toHaveLength(1);
  expect(screen.getByText('Weekly').closest('.source-name')).toHaveTextContent('(not scored)');
  expect(screen.getByText('Shows').closest('.source-name')).toHaveTextContent('(series)');
  expect(screen.getByText('Daily').closest('.source-name')).not.toHaveTextContent('(');
});

it('waits for confirmation and allows cancelling removal', async () => {
  const onRemove = vi.fn();
  const onClose = vi.fn();
  render(
    <TestWrapper>
      <RemoveScoringSource source={source} onRemove={onRemove} onClose={onClose} />
    </TestWrapper>,
  );
  expect(onRemove).not.toHaveBeenCalled();
  await userEvent.click(screen.getByRole('button', { name: 'Cancel', hidden: true }));
  expect(onClose).toHaveBeenCalled();
  expect(onRemove).not.toHaveBeenCalled();
});
