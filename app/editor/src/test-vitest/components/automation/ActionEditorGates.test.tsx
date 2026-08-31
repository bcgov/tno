import { fireEvent, render, screen } from '@testing-library/react';
import { ActionEditor } from 'features/admin/automation/designer';
import React from 'react';
import { TestWrapper } from 'test/utils';

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
    // eslint-disable-next-line no-console
    console.log('MENU:', document.body.textContent?.slice(0, 600));
    expect(screen.getByText("'dedupe' found a duplicate")).toBeInTheDocument();
    expect(screen.getByText("'dedupe' found no duplicate")).toBeInTheDocument();
  });
});
