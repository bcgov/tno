import React from 'react';
import { type IOptionItem, Select } from 'tno-core';

import { createOption, findOptionByValue } from '../utils';
import { ComboBox } from './ComboBox';
import { fitSelectWidth } from './constants';

export interface IScopedNameFieldProps {
  name: string;
  label: string;
  /** The scope the name lives in; stored as '<scope>.<bareName>'. */
  scope: '$run' | '$item';
  /** The full stored value (e.g. '$run.inbox'). */
  value?: string | null;
  onChange: (value: string | null) => void;
  /** Known full names to offer as a dropdown (e.g. every collection the definition creates). */
  knownNames?: string[];
  /** Whether a new name can be typed (producers); consumers pick from known names only. */
  allowNew?: boolean;
  /** Placeholder for the known-names dropdown. */
  placeholder?: string;
  help?: string | null;
}

/** Strip the scope prefix (and any sigil the user typed) down to the bare name. */
const toBare = (value: string | null | undefined, scope: string): string => {
  if (!value) return '';
  const trimmed = value.trim();
  if (trimmed.toLowerCase().startsWith(`${scope}.`.toLowerCase()))
    return trimmed.slice(scope.length + 1);
  // Tolerate a pasted name from the other scope or a stray sigil.
  return trimmed.replace(/^\$(run|item)\./i, '').replace(/^\$/, '');
};

/**
 * A scoped runtime name. Producers (allowNew) render a standard combobox: prior collection names
 * as suggestions, freeform text for a new name. Consumers render a pick-only dropdown of the
 * known names. Either way the field shows bare names; the stored document keeps the explicit
 * '<scope>.<name>' form the engine and validator read.
 */
export const ScopedNameField: React.FC<IScopedNameFieldProps> = ({
  name,
  label,
  scope,
  value,
  onChange,
  knownNames = [],
  allowNew = true,
  placeholder,
  help,
}) => {
  const bareNames = knownNames.map((known) => toBare(known, scope));

  if (allowNew)
    return (
      <div className="automation-scoped-name frm-in">
        <label htmlFor={`sel-${name}`}>{label}</label>
        <ComboBox
          name={name}
          placeholder={placeholder}
          // extraCh covers the clear indicator; the floor keeps empty-suggestion fields
          // (e.g. 'as (new draft)') wide enough to type a name into.
          width={fitSelectWidth(bareNames, placeholder, 4, 24)}
          isClearable
          suggestions={bareNames}
          value={toBare(value, scope)}
          onChange={(next) => {
            const bare = toBare(next, scope);
            onChange(bare ? `${scope}.${bare}` : null);
          }}
        />
        {!!help && <p className="automation-field-help">{help}</p>}
      </div>
    );

  const options: IOptionItem[] = knownNames.map((known, index) =>
    createOption(bareNames[index], known),
  );
  return (
    <Select
      name={name}
      label={label}
      width={fitSelectWidth(bareNames, placeholder)}
      isClearable={false}
      placeholder={placeholder}
      options={options}
      value={findOptionByValue(options, value) ?? ''}
      onChange={(newValue) => {
        const option = newValue as IOptionItem;
        onChange(option?.value ? `${option.value}` : null);
      }}
    >
      {!!help && <p className="automation-field-help">{help}</p>}
    </Select>
  );
};
