import React from 'react';
import { FaEdit, FaPlus } from 'react-icons/fa';
import { type IOptionItem, Row, Select, Show } from 'tno-core';

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
 * A filter picker per the design: the pencil sits inside the control (left of the clear/arrow
 * indicators, only when a filter is selected) and opens the filter in a new tab; the attached
 * plus opens a blank filter form in a new tab. The profile form refreshes its filter options when
 * the window regains focus, so a filter saved in the other tab appears on return.
 */
export const V2FilterField: React.FC<IV2FilterFieldProps> = ({
  name,
  label = 'Filter',
  value,
  options,
  onChange,
  width = '16rem',
}) => (
  <Row gap="0" alignItems="flex-end" nowrap className="v2-filter-field">
    <span className="v2-filter-select-wrap">
      <Select
        name={name}
        label={label}
        width={width}
        placeholder="Pick a filter"
        options={options}
        value={findOptionByValue(options, value) ?? ''}
        onChange={(newValue) => onChange(toNumberOrUndefined(newValue as IOptionItem))}
      />
      <Show visible={!!value}>
        <button
          type="button"
          className="v2-filter-edit"
          aria-label="Edit the selected filter in a new tab"
          title="Edit the selected filter in a new tab"
          onClick={() => {
            if (value) window.open(`/admin/filters/${value}`, '_blank', 'noopener');
          }}
        >
          <FaEdit />
        </button>
      </Show>
    </span>
    <button
      type="button"
      className="v2-filter-add"
      aria-label="Create a new filter in a new tab"
      title="Create a new filter in a new tab"
      onClick={() => window.open('/admin/filters/0', '_blank', 'noopener')}
    >
      <FaPlus />
    </button>
  </Row>
);
