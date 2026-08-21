import React from 'react';
import { FaMinusCircle } from 'react-icons/fa';
import { Col, type IOptionItem, Row, Select, Show } from 'tno-core';

import { createOption, findOptionByValue } from '../utils';
import { ComboBox } from './ComboBox';
import { conditionOpOptions, contentTokenFieldOptions, LIST_OPS, VALUELESS_OPS } from './constants';
import { DraftText } from './DraftText';
import { type IAutomationCondition } from './interfaces';

type ConditionShape = 'leaf' | 'all' | 'any' | 'not' | 'from';

const shapeOptions: IOptionItem[] = [
  createOption('Field test', 'leaf'),
  createOption('All of…', 'all'),
  createOption('Any of…', 'any'),
  createOption('Not…', 'not'),
  createOption('Analysis answer', 'from'),
];

const getShape = (condition: IAutomationCondition): ConditionShape => {
  if (condition.all) return 'all';
  if (condition.any) return 'any';
  if (condition.not) return 'not';
  // Presence, not truthiness: selecting the shape seeds an empty 'from' the user then fills.
  if (condition.from != null) return 'from';
  return 'leaf';
};

/** Split a comma list honouring backslash escapes: '\,' is a literal comma, '\\' a literal
 * backslash. Everything else (quotes included) is kept exactly as typed. */
const splitEscaped = (text: string): string[] => {
  const items: string[] = [];
  let current = '';
  for (let i = 0; i < text.length; i++) {
    const character = text[i];
    if (
      character === '\\' &&
      i + 1 < text.length &&
      (text[i + 1] === ',' || text[i + 1] === '\\')
    ) {
      current += text[i + 1];
      i++;
    } else if (character === ',') {
      items.push(current);
      current = '';
    } else current += character;
  }
  items.push(current);
  return items.map((item) => item.trim()).filter((item) => item.length > 0);
};

/** Render a condition value for editing: lists join with commas (escaping literal commas and
 * backslashes so the text round-trips), scalars stringify. */
const valueToText = (value: unknown): string => {
  if (value === undefined || value === null) return '';
  if (Array.isArray(value))
    return value.map((item) => `${item}`.replace(/\\/g, '\\\\').replace(/,/g, '\\,')).join(', ');
  return `${value}`;
};

/** Parse edited text back to a condition value: lists split on unescaped commas, numbers stay
 * numbers. */
const textToValue = (text: string, op?: string | null): unknown => {
  const trimmed = text.trim();
  if (LIST_OPS.includes(op ?? '')) return splitEscaped(trimmed);
  if (trimmed !== '' && !Number.isNaN(Number(trimmed))) return Number(trimmed);
  return trimmed;
};

export interface IConditionBuilderProps {
  value: IAutomationCondition;
  onChange: (condition: IAutomationCondition) => void;
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
export const ConditionBuilder: React.FC<IConditionBuilderProps> = ({
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
    <Col className="automation-condition" gap="0.25rem">
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
          <ComboBox
            name={`condition-field-${depth}`}
            placeholder="field"
            width="9rem"
            suggestions={contentTokenFieldOptions.map((option) => `${option.value}`)}
            value={value.field ?? ''}
            onChange={(field) => onChange({ ...value, field })}
          />
          <Select
            name={`condition-op-${depth}`}
            width="10rem"
            isClearable={false}
            options={conditionOpOptions}
            value={findOptionByValue(conditionOpOptions, value.op ?? 'equals')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              onChange({ ...value, op: `${option?.value ?? 'equals'}` });
            }}
          />
          <Show visible={!VALUELESS_OPS.includes(value.op ?? '')}>
            <DraftText
              name={`condition-value-${depth}`}
              placeholder={
                LIST_OPS.includes(value.op ?? '') ? 'comma-separated (\\, escapes)' : 'value'
              }
              canonical={valueToText(value.value)}
              width="10rem"
              onText={(text) => onChange({ ...value, value: textToValue(text, value.op) })}
            />
          </Show>
        </Show>
        <Show visible={shape === 'from'}>
          <ComboBox
            name={`condition-from-${depth}`}
            placeholder="analysisName.key"
            width="14rem"
            suggestions={fromSuggestions}
            value={value.from ?? ''}
            onChange={(from) => onChange({ from })}
          />
        </Show>
      </Row>
      <Show visible={shape === 'all' || shape === 'any'}>
        <Col className="automation-condition-children" gap="0.5rem">
          {children.map((child, index) => (
            <Row key={index} gap="0.5rem" alignItems="flex-start" nowrap>
              <ConditionBuilder
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
                className="automation-condition-remove"
                title="Remove this condition"
                onClick={() => {
                  const remaining = children.filter((_, i) => i !== index);
                  // An empty combinator is invalid; removing the last row collapses the group
                  // back to a blank field test.
                  onChange(
                    remaining.length > 0
                      ? { [listKey]: remaining }
                      : { field: '', op: 'equals', value: '' },
                  );
                }}
              >
                <FaMinusCircle />
              </button>
            </Row>
          ))}
          <button
            type="button"
            className="automation-condition-add"
            onClick={() =>
              onChange({ [listKey]: [...children, { field: '', op: 'equals', value: '' }] })
            }
          >
            + condition
          </button>
        </Col>
      </Show>
      <Show visible={shape === 'not'}>
        <Col className="automation-condition-children">
          <ConditionBuilder
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
