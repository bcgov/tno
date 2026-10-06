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
