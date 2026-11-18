import React from 'react';

import { ComboBox } from './ComboBox';
import { fitSelectWidth } from './constants';

export interface IFieldsPickerProps {
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
export const FieldsPicker: React.FC<IFieldsPickerProps> = ({
  name,
  label,
  values,
  suggestions,
  onChange,
  placeholder = 'Add a field…',
}) => {
  const remaining = suggestions.filter((suggestion) => !values.includes(suggestion));
  return (
    <div className="automation-fields-picker frm-in">
      <label>{label}</label>
      <ComboBox
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
        <div className="automation-chips">
          {values.map((value) => (
            <span key={value} className="automation-chip">
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
    </div>
  );
};
