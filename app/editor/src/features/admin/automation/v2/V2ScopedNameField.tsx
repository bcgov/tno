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
 * A scoped runtime name: the scope is a fixed, visible prefix — never typed — and known names
 * offer as a dropdown, with a bare-name input only where a new name may be created. The stored
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
  help,
}) => {
  const options: IOptionItem[] = knownNames.map((known) =>
    createOption(toBare(known, scope), known),
  );
  const bare = toBare(value, scope);

  return (
    <div className="v2-scoped-name">
      <label>{label}</label>
      <Row gap="0.5rem" alignItems="center" nowrap>
        <Show visible={options.length > 0}>
          <Select
            name={`${name}-known`}
            width="12rem"
            placeholder={allowNew ? 'pick existing…' : 'pick…'}
            options={options}
            value={findOptionByValue(options, value) ?? ''}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              onChange(option?.value ? `${option.value}` : null);
            }}
          />
        </Show>
        <Show visible={allowNew}>
          <Row alignItems="center" nowrap className="v2-scoped-name-input">
            <span className="v2-scope-prefix">{scope}.</span>
            <Text
              name={name}
              placeholder="name"
              width="10rem"
              value={bare}
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
