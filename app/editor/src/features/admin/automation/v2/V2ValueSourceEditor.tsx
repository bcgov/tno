import React from 'react';
import { type IOptionItem, Row, Select, Show, Text } from 'tno-core';

import { createOption, findOptionByValue } from '../utils';
import { type IV2ValueSource } from './interfaces';

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

export interface IV2ValueSourceEditorProps {
  name: string;
  value?: IV2ValueSource | null;
  onChange: (value: IV2ValueSource) => void;
}

/**
 * Editor for a value source: 'analysisName.key' or 'content.field', a literal, or a token
 * template. A fixed set of sources — there is nothing to compute.
 */
export const V2ValueSourceEditor: React.FC<IV2ValueSourceEditorProps> = ({
  name,
  value,
  onChange,
}) => {
  const kind = getKind(value);
  return (
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
        <Text
          name={`${name}-from`}
          placeholder="triage.sentiment or content.byline"
          value={value?.from ?? ''}
          width="20rem"
          onChange={(e) => onChange({ from: e.target.value })}
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
            const numeric = text.trim() !== '' && !Number.isNaN(Number(text)) ? Number(text) : text;
            onChange({ literal: numeric });
          }}
        />
      </Show>
      <Show visible={kind === 'template'}>
        <Text
          name={`${name}-template`}
          placeholder="DIGEST: {content.headline}"
          value={value?.template ?? ''}
          width="20rem"
          onChange={(e) => onChange({ template: e.target.value })}
        />
      </Show>
    </Row>
  );
};
