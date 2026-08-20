import React from 'react';
import { FaMinusCircle } from 'react-icons/fa';
import { Col, type IOptionItem, Row, Select, Show, Text } from 'tno-core';

import { createOption, findOptionByValue } from '../utils';
import {
  V2_LIST_OPS,
  V2_VALUELESS_OPS,
  v2ConditionOpOptions,
  v2ContentFieldOptions,
} from './constants';
import { type IV2Condition } from './interfaces';
import { V2ComboBox } from './V2ComboBox';

type ConditionShape = 'leaf' | 'all' | 'any' | 'not' | 'from';

const shapeOptions: IOptionItem[] = [
  createOption('Field test', 'leaf'),
  createOption('All of…', 'all'),
  createOption('Any of…', 'any'),
  createOption('Not…', 'not'),
  createOption('Analysis answer', 'from'),
];

const getShape = (condition: IV2Condition): ConditionShape => {
  if (condition.all) return 'all';
  if (condition.any) return 'any';
  if (condition.not) return 'not';
  // Presence, not truthiness: selecting the shape seeds an empty 'from' the user then fills.
  if (condition.from != null) return 'from';
  return 'leaf';
};

/** Render a condition value for editing: lists join with commas, scalars stringify. */
const valueToText = (value: unknown): string => {
  if (value === undefined || value === null) return '';
  if (Array.isArray(value)) return value.map((item) => `${item}`).join(', ');
  return `${value}`;
};

/** Parse edited text back to a condition value: lists split on commas, numbers stay numbers. */
const textToValue = (text: string, op?: string | null): unknown => {
  const trimmed = text.trim();
  if (V2_LIST_OPS.includes(op ?? ''))
    return trimmed
      .split(',')
      .map((item) => item.trim())
      .filter((item) => item.length > 0);
  if (trimmed !== '' && !Number.isNaN(Number(trimmed))) return Number(trimmed);
  return trimmed;
};

export interface IV2ConditionBuilderProps {
  value: IV2Condition;
  onChange: (condition: IV2Condition) => void;
  /** Nesting depth; combinator children beyond a sane depth only offer leaves. */
  depth?: number;
  /** 'analysisName.key' references whose declared type is bool, for the Analysis answer
   * autocomplete (freeform still allowed). */
  fromSuggestions?: string[];
}

/**
 * Recursive editor for a declarative condition: a field test, a combinator (all/any/not), or a
 * boolean analysis answer. There is no expression language — only these shapes.
 */
export const V2ConditionBuilder: React.FC<IV2ConditionBuilderProps> = ({
  value,
  onChange,
  depth = 0,
  fromSuggestions = [],
}) => {
  const shape = getShape(value);

  const setShape = (next: ConditionShape) => {
    switch (next) {
      case 'all':
        onChange({ all: [{ field: '', op: 'equals', value: '' }] });
        break;
      case 'any':
        onChange({ any: [{ field: '', op: 'equals', value: '' }] });
        break;
      case 'not':
        onChange({ not: { field: '', op: 'equals', value: '' } });
        break;
      case 'from':
        onChange({ from: '' });
        break;
      default:
        onChange({ field: '', op: 'equals', value: '' });
        break;
    }
  };

  const children = value.all ?? value.any ?? [];
  const listKey = value.all ? 'all' : 'any';

  return (
    <Col className="v2-condition" gap="0.25rem">
      <Row gap="0.5rem" alignItems="center" nowrap>
        <Select
          name={`condition-shape-${depth}`}
          width="10rem"
          isClearable={false}
          options={shapeOptions}
          value={findOptionByValue(shapeOptions, shape)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            if (option?.value) setShape(option.value as ConditionShape);
          }}
        />
        <Show visible={shape === 'leaf'}>
          <V2ComboBox
            name={`condition-field-${depth}`}
            placeholder="field"
            width="9rem"
            suggestions={v2ContentFieldOptions.map((option) => `${option.value}`)}
            value={value.field ?? ''}
            onChange={(field) => onChange({ ...value, field })}
          />
          <Select
            name={`condition-op-${depth}`}
            width="10rem"
            isClearable={false}
            options={v2ConditionOpOptions}
            value={findOptionByValue(v2ConditionOpOptions, value.op ?? 'equals')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              onChange({ ...value, op: `${option?.value ?? 'equals'}` });
            }}
          />
          <Show visible={!V2_VALUELESS_OPS.includes(value.op ?? '')}>
            <Text
              name={`condition-value-${depth}`}
              placeholder={
                V2_LIST_OPS.includes(value.op ?? '') ? 'comma-separated values' : 'value'
              }
              value={valueToText(value.value)}
              width="10rem"
              onChange={(e) => onChange({ ...value, value: textToValue(e.target.value, value.op) })}
            />
          </Show>
        </Show>
        <Show visible={shape === 'from'}>
          <V2ComboBox
            name={`condition-from-${depth}`}
            placeholder="analysisName.key (a boolean result)"
            width="14rem"
            suggestions={fromSuggestions}
            value={value.from ?? ''}
            onChange={(from) => onChange({ from })}
          />
        </Show>
      </Row>
      <Show visible={shape === 'all' || shape === 'any'}>
        <Col className="v2-condition-children" gap="0.5rem">
          {children.map((child, index) => (
            <Row key={index} gap="0.5rem" alignItems="flex-start" nowrap>
              <V2ConditionBuilder
                value={child}
                depth={depth + 1}
                fromSuggestions={fromSuggestions}
                onChange={(next) => {
                  const updated = [...children];
                  updated[index] = next;
                  onChange({ [listKey]: updated });
                }}
              />
              <button
                type="button"
                className="v2-condition-remove"
                title="Remove this condition"
                disabled={children.length <= 1}
                onClick={() => onChange({ [listKey]: children.filter((_, i) => i !== index) })}
              >
                <FaMinusCircle />
              </button>
            </Row>
          ))}
          <button
            type="button"
            className="v2-condition-add"
            onClick={() =>
              onChange({ [listKey]: [...children, { field: '', op: 'equals', value: '' }] })
            }
          >
            + condition
          </button>
        </Col>
      </Show>
      <Show visible={shape === 'not'}>
        <Col className="v2-condition-children">
          <V2ConditionBuilder
            value={value.not ?? { field: '', op: 'equals', value: '' }}
            depth={depth + 1}
            fromSuggestions={fromSuggestions}
            onChange={(next) => onChange({ not: next })}
          />
        </Col>
      </Show>
    </Col>
  );
};
