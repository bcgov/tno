import { render } from '@testing-library/react';
import { createOption } from 'features/admin/automation/utils';
import { V2FilterField } from 'features/admin/automation/v2';
import React from 'react';
import { TestWrapper } from 'test/utils';

const options = [createOption('Vancouver Sun', 12), createOption('CPNEWS', 18)];

/**
 * Structural contract for the filter field design:
 * - one control: the select with the edit pencil rendered INSIDE its indicators area
 *   (left of the clear/dropdown indicators), never outside or below it;
 * - the pencil only exists when a filter is selected;
 * - the add (+) button sits in the same control row as a sibling of the select.
 */
describe('V2FilterField', () => {
  it('renders the add button in the control row and no pencil when nothing is selected', () => {
    const { container } = render(
      <TestWrapper>
        <V2FilterField name="test-filter" options={options} value={undefined} onChange={() => {}} />
      </TestWrapper>,
    );
    expect(container.querySelector('.v2-filter-add')).not.toBeNull();
    expect(container.querySelector('.v2-filter-edit')).toBeNull();
  });

  it('renders the pencil inside the select control indicators when a filter is selected', () => {
    const { container } = render(
      <TestWrapper>
        <V2FilterField name="test-filter" options={options} value={12} onChange={() => {}} />
      </TestWrapper>,
    );
    const edit = container.querySelector('.v2-filter-edit');
    expect(edit).not.toBeNull();
    // The pencil is in-flow content of react-select's indicators container (inside the control
    // border, beside the clear x and dropdown arrow) - not an overlay outside the control.
    const indicators = container.querySelector('.rs__indicators');
    expect(indicators).not.toBeNull();
    expect(indicators!.contains(edit)).toBe(true);
    const control = container.querySelector('.rs__control');
    expect(control!.contains(edit)).toBe(true);
    // The clear and dropdown indicators are present with it, pencil first.
    expect(indicators!.querySelector('.rs__dropdown-indicator')).not.toBeNull();
    expect(indicators!.firstElementChild!.classList.contains('v2-filter-edit')).toBe(true);
  });

  it('keeps the add button outside the select but beside it', () => {
    const { container } = render(
      <TestWrapper>
        <V2FilterField name="test-filter" options={options} value={12} onChange={() => {}} />
      </TestWrapper>,
    );
    const add = container.querySelector('.v2-filter-add');
    const control = container.querySelector('.rs__control');
    expect(add).not.toBeNull();
    // Outside the select's border, inside the field's own control row (the Select children slot).
    expect(control!.contains(add)).toBe(false);
    expect(container.querySelector('.frm-in')!.contains(add)).toBe(true);
  });
});
