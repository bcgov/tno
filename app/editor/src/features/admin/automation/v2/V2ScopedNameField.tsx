import React from 'react';
import { type IOptionItem, Row, Select, Text } from 'tno-core';

import { createOption, findOptionByValue } from '../utils';
import { fitSelectWidth } from './constants';

export interface IV2ScopedNameFieldProps {
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

/** The dropdown sentinel that reveals the new-name input. */
const NEW_NAME = '__new__';

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
 * A scoped runtime name rendered as a standard labelled field: the tno-core Select owns the
 * label (so it styles like every other field) and lists the known names; when new names are
 * allowed, a 'New…' option reveals a bare-name input behind a fixed scope prefix in the field's
 * control row. The scope is never typed; the stored document keeps the explicit '<scope>.<name>'
 * form the engine and validator read. The field sizes to fit the values in its dropdown.
 */
export const V2ScopedNameField: React.FC<IV2ScopedNameFieldProps> = ({
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
  const isKnown = !!value && knownNames.includes(value);
  // Typing a new name is a mode: entered via the 'New…' option, exited by picking a known name.
  // Starts on when the current value is a custom name the dropdown cannot represent.
  const [isCreating, setIsCreating] = React.useState(!!value && !isKnown);
  const showInput = allowNew && (knownNames.length === 0 || isCreating);

  const bareNames = knownNames.map((known) => toBare(known, scope));
  const options: IOptionItem[] = [
    ...knownNames.map((known, index) => createOption(bareNames[index], known)),
    ...(allowNew ? [createOption('➕ New…', NEW_NAME)] : []),
  ];
  const selected = isCreating
    ? findOptionByValue(options, NEW_NAME)
    : findOptionByValue(options, value) ?? '';

  return (
    <Select
      name={name}
      label={label}
      width={fitSelectWidth(bareNames, placeholder)}
      isClearable={false}
      placeholder={placeholder}
      options={options}
      value={selected}
      onChange={(newValue) => {
        const option = newValue as IOptionItem;
        if (option?.value === NEW_NAME) {
          setIsCreating(true);
          onChange(null);
        } else {
          setIsCreating(false);
          onChange(option?.value ? `${option.value}` : null);
        }
      }}
    >
      {showInput && (
        <Row alignItems="center" nowrap className="v2-scoped-name-input">
          <span className="v2-scope-prefix">{scope}.</span>
          <Text
            name={`${name}-new`}
            placeholder="name"
            width="10rem"
            value={toBare(value, scope)}
            onChange={(e) => {
              const next = toBare(e.target.value, scope);
              onChange(next ? `${scope}.${next}` : null);
            }}
          />
        </Row>
      )}
      {!!help && <p className="v2-field-help">{help}</p>}
    </Select>
  );
};
