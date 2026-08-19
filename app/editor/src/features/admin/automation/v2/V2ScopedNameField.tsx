import React from 'react';
import { type IOptionItem, Row, Select, Show, Text } from 'tno-core';

import { createOption, findOptionByValue } from '../utils';

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
 * A scoped runtime name rendered as one control: a dropdown of the known names when any exist
 * (with a 'New…' option revealing a bare-name input behind a fixed scope prefix), or just the
 * prefixed input when there is nothing to pick from. The scope is never typed; the stored
 * document keeps the explicit '<scope>.<name>' form the engine and validator read.
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
  const hasDropdown = knownNames.length > 0;
  const showInput = allowNew && (!hasDropdown || isCreating);

  const options: IOptionItem[] = [
    ...knownNames.map((known) => createOption(toBare(known, scope), known)),
    ...(allowNew ? [createOption('➕ New…', NEW_NAME)] : []),
  ];
  const selected = isCreating
    ? findOptionByValue(options, NEW_NAME)
    : findOptionByValue(options, value) ?? '';

  return (
    <div className="v2-scoped-name">
      <label>{label}</label>
      <Row gap="0.5rem" alignItems="center" nowrap>
        <Show visible={hasDropdown}>
          <Select
            name={`${name}-pick`}
            width="12rem"
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
          />
        </Show>
        <Show visible={showInput}>
          <Row alignItems="center" nowrap className="v2-scoped-name-input">
            <span className="v2-scope-prefix">{scope}.</span>
            <Text
              name={name}
              placeholder="name"
              width="10rem"
              value={toBare(value, scope)}
              onChange={(e) => {
                const next = toBare(e.target.value, scope);
                onChange(next ? `${scope}.${next}` : null);
              }}
            />
          </Row>
        </Show>
      </Row>
      <Show visible={!!help}>
        <p className="v2-field-help">{help}</p>
      </Show>
    </div>
  );
};
