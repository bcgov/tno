import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { defaultTopicScoreRule } from 'features/admin/topic-score-rules/constants';
import { RuleDrawer } from 'features/admin/topic-score-rules/RuleDrawer';
import { RulesPane } from 'features/admin/topic-score-rules/RulesPane';
import React from 'react';
import { TestWrapper } from 'test/utils';
import { vi } from 'vitest';

vi.mock('store/hooks', () => ({
  useApp: () => [{ requests: [] }],
  useLookup: () => [
    {
      series: [
        { id: 10, sourceId: 1, name: 'Morning' },
        { id: 11, sourceId: 1, name: 'Evening' },
        { id: 20, sourceId: 2, name: 'Weekend' },
        { id: 30, sourceId: null, name: 'Shared columnist' },
      ],
    },
  ],
}));

const source = {
  id: 1,
  name: 'Radio',
  code: 'RAD',
  useInTopics: true,
  seriesUseInTopics: false,
  ruleCount: 2,
};
const rules = [
  { id: 1, sourceId: 1, seriesId: 10, score: 60, sortOrder: 0 },
  { id: 2, sourceId: 1, score: 40, sortOrder: 1 },
];
const props = {
  source,
  rules,
  onAdd: vi.fn(),
  onEdit: vi.fn(),
  onDelete: vi.fn(),
  onReorder: vi.fn(),
};

it('offers all source series, filters rules, and restores the full list with All series', async () => {
  const user = userEvent.setup();
  render(
    <TestWrapper>
      <RulesPane {...props} />
    </TestWrapper>,
  );
  await screen.findByText('60');
  await user.click(screen.getByRole('combobox'));
  expect(screen.getByRole('option', { name: 'Evening' })).toBeInTheDocument();
  expect(screen.queryByRole('option', { name: 'Weekend' })).not.toBeInTheDocument();
  await user.click(screen.getByRole('option', { name: 'Morning' }));
  expect(screen.getByText('60')).toBeInTheDocument();
  expect(screen.queryByText('40')).not.toBeInTheDocument();
  await user.click(screen.getByRole('combobox'));
  await user.click(screen.getByRole('option', { name: 'All series' }));
  expect(screen.getByText('40')).toBeInTheDocument();
});

it('clears stale series and section filters when switching sources', async () => {
  const user = userEvent.setup();
  const { rerender } = render(
    <TestWrapper>
      <RulesPane {...props} />
    </TestWrapper>,
  );
  await user.click(screen.getByRole('combobox'));
  await user.click(screen.getByRole('option', { name: 'Morning' }));
  await user.type(screen.getByLabelText('Section'), 'A');
  rerender(
    <TestWrapper>
      <RulesPane
        {...props}
        source={{ ...source, id: 2 }}
        rules={[{ id: 3, sourceId: 2, seriesId: 20, score: 90, sortOrder: 0 }]}
      />
    </TestWrapper>,
  );
  await waitFor(() => expect(screen.getByLabelText('Section')).toHaveValue(''));
  expect(screen.getByText('All series')).toBeInTheDocument();
  expect(await screen.findByText('90')).toBeInTheDocument();
});

it('edits through an icon and deletes only after confirmation', async () => {
  const user = userEvent.setup();
  const onDelete = vi.fn().mockResolvedValue(undefined);
  const onEdit = vi.fn();
  render(
    <TestWrapper>
      <RulesPane {...props} onDelete={onDelete} onEdit={onEdit} />
    </TestWrapper>,
  );
  await user.click(await screen.findByRole('button', { name: 'Edit rule 1' }));
  expect(onEdit).toHaveBeenCalledWith(rules[0]);
  await user.click(screen.getByRole('button', { name: 'Delete rule 1' }));
  expect(onDelete).not.toHaveBeenCalled();
  await user.click(screen.getByRole('button', { name: 'Cancel', hidden: true }));
  expect(onDelete).not.toHaveBeenCalled();
  await user.click(screen.getByRole('button', { name: 'Delete rule 1' }));
  await user.click(screen.getByRole('button', { name: 'Delete rule', hidden: true }));
  await waitFor(() => expect(onDelete).toHaveBeenCalledWith(rules[0]));
});

it('offers shared series in the filter when the source has no assigned series', async () => {
  const user = userEvent.setup();
  render(
    <TestWrapper>
      <RulesPane {...props} source={{ ...source, id: 3 }} rules={[]} />
    </TestWrapper>,
  );
  await user.click(screen.getByRole('combobox'));
  expect(screen.getByRole('option', { name: 'Shared columnist' })).toBeInTheDocument();
  expect(screen.queryByRole('option', { name: 'Morning' })).not.toBeInTheDocument();
});

it('allows a shared series to be selected and saved in the rule form', async () => {
  const user = userEvent.setup();
  const onSave = vi.fn().mockResolvedValue(undefined);
  render(
    <TestWrapper>
      <RuleDrawer
        source={{ ...source, id: 3 }}
        rule={defaultTopicScoreRule(3)}
        sections={[]}
        onSave={onSave}
        onDelete={vi.fn()}
        onClose={vi.fn()}
      />
    </TestWrapper>,
  );
  await user.click(screen.getAllByRole('combobox')[0]);
  expect(screen.queryByRole('option', { name: 'Weekend' })).not.toBeInTheDocument();
  await user.click(screen.getByRole('option', { name: 'Shared columnist' }));
  await user.click(screen.getByRole('button', { name: 'Save' }));
  await waitFor(() =>
    expect(onSave).toHaveBeenCalledWith(expect.objectContaining({ sourceId: 3, seriesId: 30 })),
  );
});
