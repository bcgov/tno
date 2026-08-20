import React from 'react';
import { Col } from 'tno-core';

import { fitSelectWidth } from './constants';
import { V2ComboBox } from './V2ComboBox';

export interface IV2FieldsPickerProps {
  name: string;
  label: string;
  /** The selected field names, rendered as removable chips under the picker. */
  values: string[];
  /** Known property names offered in the dropdown; freeform text is also accepted. */
  suggestions: string[];
  onChange: (values: string[]) => void;
  placeholder?: string;
}

/**
 * A multi-value field picker: a standard combobox that adds the chosen (or typed) property to
 * the list, with the selections shown as removable chips below the control.
 */
export const V2FieldsPicker: React.FC<IV2FieldsPickerProps> = ({
  name,
  label,
  values,
  suggestions,
  onChange,
  placeholder = 'Add a field…',
}) => {
  const remaining = suggestions.filter((suggestion) => !values.includes(suggestion));
  return (
    <Col className="v2-fields-picker frm-in">
      <label>{label}</label>
      <V2ComboBox
        name={name}
        placeholder={placeholder}
        width={fitSelectWidth(suggestions, placeholder)}
        suggestions={remaining}
        value=""
        onChange={(next) => {
          const trimmed = next.trim();
          if (trimmed && !values.includes(trimmed)) onChange([...values, trimmed]);
        }}
      />
      {values.length > 0 && (
        <div className="v2-chips">
          {values.map((value) => (
            <span key={value} className="v2-chip">
              {value}
              <button
                type="button"
                title={`Remove ${value}`}
                onClick={() => onChange(values.filter((item) => item !== value))}
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}
    </Col>
  );
};
