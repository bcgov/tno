import React from 'react';
import { type IOptionItem, Row, Select, Show, Text } from 'tno-core';

import { createOption, findOptionByValue } from '../utils';
import { type IV2ValueSource } from './interfaces';
import { V2ComboBox } from './V2ComboBox';

type ValueKind = 'from' | 'literal' | 'template';

const kindOptions: IOptionItem[] = [
  createOption('Analysis result / content field', 'from'),
  createOption('Literal value', 'literal'),
  createOption('Template', 'template'),
];

const getKind = (value?: IV2ValueSource | null): ValueKind => {
  if (value?.template != null) return 'template';
  if (value?.literal !== undefined) return 'literal';
  return 'from';
};

/** Full explanation of the selected source, shown under the row. */
export const kindHelp: Record<ValueKind, string> = {
  from: "Reads a named result: 'analysisName.key' takes the answer from one of this step's analyses (the analysis runs when first used - one LLM call per item); 'content.field' copies a field from the item's working copy, changes included.",
  literal:
    'Applies this exact value to every item. A plain number is stored as a number (e.g. 0); anything else is text.',
  template:
    "Builds text per item: every {content.*} token is replaced with that item's value and the finished text is stored. Example: [{content.source.code}] {content.headline} becomes [SUN] Mayor announces budget.",
};

export interface IV2ValueSourceEditorProps {
  name: string;
  value?: IV2ValueSource | null;
  /** 'analysisName.key' / 'content.field' references offered by the from autocomplete. */
  fromSuggestions?: string[];
  /** Hide the per-row kind help (the caller renders it once for the whole section). */
  showHelp?: boolean;
  onChange: (value: IV2ValueSource) => void;
}

/**
 * Editor for a value source: 'analysisName.key' or 'content.field', a literal, or a token
 * template. A fixed set of sources — there is nothing to compute.
 */
export const V2ValueSourceEditor: React.FC<IV2ValueSourceEditorProps> = ({
  name,
  value,
  fromSuggestions = [],
  showHelp = true,
  onChange,
}) => {
  const kind = getKind(value);
  return (
    <div className="v2-value-source">
      <Row gap="0.5rem" alignItems="center" nowrap>
        <Select
          name={`${name}-kind`}
          width="16rem"
          isClearable={false}
          options={kindOptions}
          value={findOptionByValue(kindOptions, kind)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            switch (option?.value) {
              case 'literal':
                onChange({ literal: '' });
                break;
              case 'template':
                onChange({ template: '' });
                break;
              default:
                onChange({ from: '' });
                break;
            }
          }}
        />
        <Show visible={kind === 'from'}>
          <V2ComboBox
            name={`${name}-from`}
            placeholder="Pick an analysis key or content field"
            width="20rem"
            suggestions={fromSuggestions}
            value={value?.from ?? ''}
            onChange={(from) => onChange({ from })}
          />
        </Show>
        <Show visible={kind === 'literal'}>
          <Text
            name={`${name}-literal`}
            placeholder="a fixed value"
            value={value?.literal === undefined ? '' : `${value.literal}`}
            width="20rem"
            onChange={(e) => {
              const text = e.target.value;
              const numeric =
                text.trim() !== '' && !Number.isNaN(Number(text)) ? Number(text) : text;
              onChange({ literal: numeric });
            }}
          />
        </Show>
        <Show visible={kind === 'template'}>
          <Text
            name={`${name}-template`}
            placeholder="Text with {content.*} tokens, e.g. [{content.source.code}] {content.headline}"
            value={value?.template ?? ''}
            width="20rem"
            onChange={(e) => onChange({ template: e.target.value })}
          />
        </Show>
      </Row>
      <Show visible={showHelp}>
        <p className="v2-field-help">{kindHelp[kind]}</p>
      </Show>
    </div>
  );
};
