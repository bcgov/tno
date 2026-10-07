import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AddScoringSource } from 'features/admin/topic-score-rules/AddScoringSource';
import React from 'react';
import { TestWrapper } from 'test/utils';
import { vi } from 'vitest';

const api = vi.hoisted(() => ({
  findAllSources: vi.fn(),
  getSource: vi.fn(),
  updateSource: vi.fn(),
}));
vi.mock('store/hooks/admin', () => ({ useSources: () => [{}, api] }));
const existing = {
  id: 1,
  code: 'ONE',
  name: 'Already scored',
  useInTopics: true,
  seriesUseInTopics: false,
  ruleCount: 1,
};
const added = {
  id: 2,
  code: 'TWO',
  name: 'New to scoring',
  useInTopics: false,
  version: 5,
  configuration: { timeZone: 'Pacific Standard Time' },
};

beforeEach(() => {
  vi.clearAllMocks();
  api.findAllSources.mockResolvedValue([existing, added]);
  api.getSource.mockResolvedValue(added);
  api.updateSource.mockResolvedValue({ ...added, useInTopics: true });
});

it('adds an existing source using its full current settings, excluding sources already listed', async () => {
  const user = userEvent.setup();
  const onAdded = vi.fn().mockResolvedValue(undefined);
  render(
    <TestWrapper>
      <AddScoringSource sources={[existing]} onAdded={onAdded} onClose={vi.fn()} />
    </TestWrapper>,
  );
  const select = screen.getByRole('combobox', { hidden: true });
  await waitFor(() => expect(select).not.toBeDisabled());
  await user.click(select);
  expect(screen.queryByRole('option', { name: 'Already scored (ONE)' })).not.toBeInTheDocument();
  await user.click(screen.getByRole('option', { name: 'New to scoring (TWO)' }));
  await user.click(screen.getByRole('button', { name: 'Add source', hidden: true }));
  await waitFor(() => expect(onAdded).toHaveBeenCalledWith(2));
  expect(api.updateSource).toHaveBeenCalledWith({ ...added, useInTopics: true });
});

it('keeps the dialog open when saving fails and provides a path to create a source', async () => {
  api.updateSource.mockRejectedValue(new Error('Save failed'));
  const user = userEvent.setup();
  const onAdded = vi.fn();
  render(
    <TestWrapper>
      <AddScoringSource sources={[]} onAdded={onAdded} onClose={vi.fn()} />
    </TestWrapper>,
  );
  expect(screen.getByRole('link', { name: 'Create a source', hidden: true })).toHaveAttribute(
    'href',
    '/admin/sources/0',
  );
  const select = screen.getByRole('combobox', { hidden: true });
  await waitFor(() => expect(select).not.toBeDisabled());
  await user.click(select);
  await user.click(screen.getByRole('option', { name: 'New to scoring (TWO)' }));
  await user.click(screen.getByRole('button', { name: 'Add source', hidden: true }));
  await waitFor(() => expect(api.updateSource).toHaveBeenCalled());
  expect(onAdded).not.toHaveBeenCalled();
  await waitFor(() =>
    expect(screen.getByRole('button', { name: 'Add source', hidden: true })).not.toBeDisabled(),
  );
});
