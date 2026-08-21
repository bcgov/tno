import { render } from '@testing-library/react';
import { FilterField } from 'features/admin/automation/designer';
import { createOption } from 'features/admin/automation/utils';
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
describe('FilterField', () => {
  it('renders the add button in the control row and no pencil when nothing is selected', () => {
    const { container } = render(
      <TestWrapper>
        <FilterField name="test-filter" options={options} value={undefined} onChange={() => {}} />
      </TestWrapper>,
    );
    // The + is always present inside the control; the pencil only with a selection.
    expect(
      container.querySelector('.rs__control')!.querySelector('.automation-filter-add'),
    ).not.toBeNull();
    expect(container.querySelector('.automation-filter-edit')).toBeNull();
  });

  it('renders the pencil inside the select control indicators when a filter is selected', () => {
    const { container } = render(
      <TestWrapper>
        <FilterField name="test-filter" options={options} value={12} onChange={() => {}} />
      </TestWrapper>,
    );
    const edit = container.querySelector('.automation-filter-edit');
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
    expect(indicators!.firstElementChild!.classList.contains('automation-filter-edit')).toBe(true);
  });

  it('renders the add button inside the control, after the dropdown indicator', () => {
    const { container } = render(
      <TestWrapper>
        <FilterField name="test-filter" options={options} value={12} onChange={() => {}} />
      </TestWrapper>,
    );
    const add = container.querySelector('.automation-filter-add');
    const indicators = container.querySelector('.rs__indicators');
    expect(add).not.toBeNull();
    // Attached: the + is part of the control itself (one component), last in the indicators.
    expect(container.querySelector('.rs__control')!.contains(add)).toBe(true);
    expect(indicators!.lastElementChild!.classList.contains('automation-filter-add')).toBe(true);
  });
});
