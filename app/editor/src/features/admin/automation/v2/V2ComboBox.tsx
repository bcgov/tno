import React from 'react';
import CreatableSelect from 'react-select/creatable';
import styled from 'styled-components';

interface IComboOption {
  label: string;
  value: string;
}

/**
 * Mirrors tno-core's SelectField control styling (primary variant) so the combobox is
 * indistinguishable from every other dropdown; the menu portals to document.body exactly like
 * tno-core's Select, so it inherits the same default menu look.
 */
const ComboBoxWrapper = styled.div<{ $width?: string }>`
  /* Carries the global .frm-in class so it gets the same wrapper padding as every tno-core
     field and lines up beside them. */

  .rs__control {
    display: flex;
    width: ${(props) => props.$width ?? '100%'};
    font-weight: 400;
    text-align: left;
    vertical-align: middle;
    user-select: text;
    border-width: 1px;
    border-style: solid;
    font-size: 1rem;
    line-height: 1.6;
    border-radius: 0.25rem;
    transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
      border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
    overflow: visible;
    text-transform: none;
    color: ${(props) => props.theme.css.primaryColor};
    background-color: ${(props) => props.theme.css.inputBackgroundColor};
    border-color: #606060;
  }

  /* Single-line control: long placeholders/values never wrap the control taller. */
  .rs__placeholder,
  .rs__single-value {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
`;

export interface IV2ComboBoxProps {
  name: string;
  /** The committed text value. */
  value: string;
  /** Suggested values shown in the dropdown; anything typed is also accepted. */
  suggestions: string[];
  onChange: (value: string) => void;
  placeholder?: string;
  width?: string;
  'aria-label'?: string;
}

/**
 * A standard-styled dropdown that also accepts freeform text: the suggestions list is a plain
 * react-select menu, and typed text commits through the create option or by leaving the field.
 */
export const V2ComboBox: React.FC<IV2ComboBoxProps> = ({
  name,
  value,
  suggestions,
  onChange,
  placeholder,
  width,
  'aria-label': ariaLabel,
}) => {
  const [inputValue, setInputValue] = React.useState('');
  const options: IComboOption[] = suggestions.map((text) => ({ label: text, value: text }));
  const selected: IComboOption | null = value ? { label: value, value } : null;

  return (
    <ComboBoxWrapper className="frm-in" $width={width}>
      <CreatableSelect<IComboOption, false>
        name={name}
        aria-label={ariaLabel ?? placeholder}
        className="frm-select"
        classNamePrefix="rs"
        placeholder={placeholder}
        options={options}
        value={selected}
        inputValue={inputValue}
        isClearable={false}
        menuPortalTarget={document.body}
        styles={{
          menuPortal: (base) => ({ ...base, zIndex: 9999 }),
          // The menu is portaled, so wrapper CSS can't reach it: options stay on one line and
          // the menu grows to fit them instead of wrapping inside the control's width.
          menu: (base) => ({ ...base, width: 'max-content', minWidth: '100%' }),
          option: (base) => ({ ...base, whiteSpace: 'nowrap' }),
        }}
        formatCreateLabel={(text) => `Use "${text}"`}
        onInputChange={(text, meta) => {
          if (meta.action === 'input-change') setInputValue(text);
        }}
        onChange={(option) => {
          setInputValue('');
          onChange(option?.value ?? '');
        }}
        onCreateOption={(text) => {
          setInputValue('');
          onChange(text.trim());
        }}
        onBlur={() => {
          // Typed text commits on leaving the field, without requiring the create option.
          const typed = inputValue.trim();
          setInputValue('');
          if (typed && typed !== value) onChange(typed);
        }}
      />
    </ComboBoxWrapper>
  );
};
