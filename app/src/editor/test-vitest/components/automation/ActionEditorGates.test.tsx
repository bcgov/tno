import { fireEvent, render, screen } from '@testing-library/react';
import { ActionEditor } from 'features/admin/automation/designer';
import React from 'react';
import { TestWrapper } from 'test/utils';
import { vi } from 'vitest';

const abortDescriptor = {
  type: 'abort',
  label: 'Stop Remaining Actions',
  category: 'flow',
  requiresSubject: true,
  requiresPersistedId: false,
  usesLLM: false,
  phases: ['process'],
  fields: [],
};

describe('ActionEditor dedupe gates', () => {
  it('offers found-a-duplicate gates when the step has a dedupe action', () => {
    render(
      <TestWrapper>
        <ActionEditor
          action={{ type: 'collection.add', isEnabled: true }}
          descriptors={[
            {
              type: 'collection.add',
              label: 'Add To Collection',
              category: 'collection',
              requiresSubject: true,
              requiresPersistedId: false,
              usesLLM: false,
              phases: ['process'],
              fields: [{ name: 'into', kind: 'collection', required: true }],
            },
          ]}
          phase="process"
          analyses={[]}
          dedupeRefs={['dedupe.isDuplicate']}
          collectionNames={['$run.dd-province']}
          draftNames={[]}
          filterOptions={[]}
          reportOptions={[]}
          notificationOptions={[]}
          actionOptions={[]}
          contentActions={[]}
          promptNames={[]}
          onChange={() => {}}
        />
      </TestWrapper>,
    );
    const gate = document.querySelector('[id="sel-action-gate"]') as HTMLElement;
    expect(gate).not.toBeNull();
    const control = gate.closest('.frm-select')?.querySelector('.rs__control') ?? gate;
    fireEvent.mouseDown(control, { button: 0 });
    expect(screen.getByText("'dedupe' found a duplicate")).toBeInTheDocument();
    expect(screen.getByText("'dedupe' found no duplicate")).toBeInTheDocument();
  });
});

describe('ActionEditor prior action gates', () => {
  const renderEditor = (
    action: React.ComponentProps<typeof ActionEditor>['action'],
    onChange: (next: any) => void = () => {},
  ) =>
    render(
      <TestWrapper>
        <ActionEditor
          action={action}
          descriptors={[abortDescriptor]}
          phase="process"
          analyses={[]}
          actionRefs={[{ name: 'Publish Content', label: 'Publish Content' }]}
          collectionNames={[]}
          draftNames={[]}
          filterOptions={[]}
          reportOptions={[]}
          notificationOptions={[]}
          actionOptions={[]}
          contentActions={[]}
          promptNames={[]}
          onChange={onChange}
        />
      </TestWrapper>,
    );

  const openSelect = (id: string) => {
    const select = document.querySelector(`[id="${id}"]`) as HTMLElement;
    expect(select).not.toBeNull();
    fireEvent.mouseDown(select.closest('.frm-select')?.querySelector('.rs__control') ?? select, {
      button: 0,
    });
  };

  it('gates on an earlier action having run', () => {
    const onChange = vi.fn();
    renderEditor({ type: 'abort', isEnabled: true }, onChange);
    openSelect('sel-action-gate');
    fireEvent.click(screen.getByText('Prior action outcome'));
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({ when: { from: 'Publish Content.ran' } }),
    );
  });

  it('reads a negated ran gate back as did not run', () => {
    renderEditor({
      type: 'abort',
      isEnabled: true,
      when: { not: { from: 'Publish Content.ran' } },
    });
    expect(document.querySelector('[id="sel-action-prior"]')).not.toBeNull();
    expect(screen.getByText('did not run')).toBeInTheDocument();
  });

  it('reads a value comparison back with its operator and value', () => {
    renderEditor({
      type: 'abort',
      isEnabled: true,
      when: { from: 'Publish Content.value', op: 'notEquals', value: 'publish' },
    });
    expect(screen.getByText('outcome value…')).toBeInTheDocument();
    expect(screen.getByText('not equals')).toBeInTheDocument();
    expect(screen.getByDisplayValue('publish')).toBeInTheDocument();
  });

  it('stores did not run as a negated ran gate', () => {
    const onChange = vi.fn();
    renderEditor(
      { type: 'abort', isEnabled: true, when: { from: 'Publish Content.ran' } },
      onChange,
    );
    openSelect('sel-action-prior-outcome');
    fireEvent.click(screen.getByText('did not run'));
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({ when: { not: { from: 'Publish Content.ran' } } }),
    );
  });

  it('offers no prior action gate when nothing runs before it', () => {
    render(
      <TestWrapper>
        <ActionEditor
          action={{ type: 'abort', isEnabled: true }}
          descriptors={[abortDescriptor]}
          phase="process"
          analyses={[]}
          collectionNames={[]}
          draftNames={[]}
          filterOptions={[]}
          reportOptions={[]}
          notificationOptions={[]}
          actionOptions={[]}
          contentActions={[]}
          promptNames={[]}
          onChange={() => {}}
        />
      </TestWrapper>,
    );
    openSelect('sel-action-gate');
    expect(screen.queryByText('Prior action outcome')).toBeNull();
  });
});
