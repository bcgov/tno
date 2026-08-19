import React from 'react';
import { FaEdit, FaPlus } from 'react-icons/fa';
import { components as selectComponents, type IndicatorsContainerProps } from 'react-select';
import { type IOptionItem, Select } from 'tno-core';

import { findOptionByValue, toNumberOrUndefined } from '../utils';

export interface IV2FilterFieldProps {
  name: string;
  label?: string;
  value?: number | null;
  options: IOptionItem[];
  onChange: (filterId: number | undefined) => void;
  width?: string;
  required?: boolean;
}

/**
 * A filter picker per the design: 'Pick a filter' placeholder; when a filter is selected an edit
 * pencil renders INSIDE the control (in the indicators area, left of the clear × and dropdown ▼)
 * and opens the filter in a new tab; a compact + beside the control opens a blank filter form in
 * a new tab. Both are injected through react-select's components API into the control's
 * indicators area - pencil before the clear/dropdown indicators, + after them - so the whole
 * thing is one attached control, exactly as designed. The profile form refreshes its filter
 * options when the window regains focus, so a filter saved in the other tab appears on return.
 */
export const V2FilterField: React.FC<IV2FilterFieldProps> = ({
  name,
  label = 'Filter',
  value,
  options,
  onChange,
  width = '16rem',
  required,
}) => {
  // Injected into the control's indicators area, before the clear/dropdown indicators.
  const IndicatorsContainer = React.useMemo(
    () =>
      function FilterIndicators(props: IndicatorsContainerProps<unknown, boolean>) {
        const selected = (props.getValue()[0] as IOptionItem | undefined)?.value;
        return (
          <selectComponents.IndicatorsContainer {...props}>
            {!!selected && (
              <button
                type="button"
                className="v2-filter-edit"
                aria-label="Edit the selected filter in a new tab"
                title="Edit the selected filter in a new tab"
                // Stop react-select from treating the click as a menu toggle.
                onMouseDown={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                }}
                onClick={() => window.open(`/admin/filters/${selected}`, '_blank', 'noopener')}
              >
                <FaEdit />
              </button>
            )}
            {props.children}
            <button
              type="button"
              className="v2-filter-add"
              aria-label="Create a new filter in a new tab"
              title="Create a new filter in a new tab"
              onMouseDown={(e) => {
                e.preventDefault();
                e.stopPropagation();
              }}
              onClick={() => window.open('/admin/filters/0', '_blank', 'noopener')}
            >
              <FaPlus />
            </button>
          </selectComponents.IndicatorsContainer>
        );
      },
    [],
  );

  return (
    <Select
      name={name}
      label={label}
      required={required}
      width={width}
      placeholder="Pick a filter"
      options={options}
      value={findOptionByValue(options, value) ?? ''}
      onChange={(newValue) => onChange(toNumberOrUndefined(newValue as IOptionItem))}
      components={{ IndicatorsContainer }}
    />
  );
};
