import React from 'react';
import { FaEdit, FaPlus } from 'react-icons/fa';
import { type IOptionItem, Row, Select } from 'tno-core';

import { findOptionByValue, toNumberOrUndefined } from '../utils';

export interface IV2FilterFieldProps {
  name: string;
  label?: string;
  value?: number | null;
  options: IOptionItem[];
  onChange: (filterId: number | undefined) => void;
  width?: string;
}

/**
 * A filter picker with edit/create shortcuts: the pencil opens the selected filter in a new tab,
 * the plus opens a blank filter form in a new tab. The profile form refreshes its filter options
 * when the window regains focus, so a filter saved in the other tab appears on return.
 */
export const V2FilterField: React.FC<IV2FilterFieldProps> = ({
  name,
  label = 'Filter',
  value,
  options,
  onChange,
  width = '18rem',
}) => (
  <Row gap="0.25rem" alignItems="flex-end" nowrap className="v2-filter-field">
    <Select
      name={name}
      label={label}
      width={width}
      options={options}
      value={findOptionByValue(options, value) ?? ''}
      onChange={(newValue) => onChange(toNumberOrUndefined(newValue as IOptionItem))}
    />
    <button
      type="button"
      className="rule-icon-button"
      aria-label="Edit the selected filter in a new tab"
      title="Edit the selected filter in a new tab"
      disabled={!value}
      onClick={() => {
        if (value) window.open(`/admin/filters/${value}`, '_blank', 'noopener');
      }}
    >
      <FaEdit />
    </button>
    <button
      type="button"
      className="rule-icon-button"
      aria-label="Create a new filter in a new tab"
      title="Create a new filter in a new tab"
      onClick={() => window.open('/admin/filters/0', '_blank', 'noopener')}
    >
      <FaPlus />
    </button>
  </Row>
);
