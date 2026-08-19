import React from 'react';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text } from 'tno-core';

import { contentFieldOptionItems } from '../constants';
import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import { type IV2Action, type IV2ActionDescriptor, type IV2FieldSpec } from './interfaces';
import { V2ConditionBuilder } from './V2ConditionBuilder';
import { V2FilterField } from './V2FilterField';
import { V2ScopedNameField } from './V2ScopedNameField';
import { V2ValueSourceEditor } from './V2ValueSourceEditor';

type GateKind = 'always' | 'condition' | 'confirm';

const gateOptions: IOptionItem[] = [
  createOption('Always run', 'always'),
  createOption('Condition (no LLM call)', 'condition'),
  createOption('LLM confirmation statement', 'confirm'),
];

const getGate = (action: IV2Action): GateKind => {
  if (action.confirm) return 'confirm';
  if (action.when) return 'condition';
  return 'always';
};

const truncateToText = (truncate?: Record<string, number> | null): string =>
  truncate
    ? Object.entries(truncate)
        .map(([field, max]) => `${field}=${max}`)
        .join(', ')
    : '';

const textToTruncate = (text: string): Record<string, number> | null => {
  const entries = text
    .split(',')
    .map((pair) => pair.split('='))
    .filter((parts) => parts.length === 2 && parts[0].trim() && !Number.isNaN(Number(parts[1])))
    .map(([field, max]) => [field.trim(), Number(max)] as const);
  return entries.length > 0 ? Object.fromEntries(entries) : null;
};

export interface IV2ActionEditorProps {
  action: IV2Action;
  descriptors: IV2ActionDescriptor[];
  phase: string;
  analysisNames: string[];
  collectionNames: string[];
  /** Drafts created by earlier content.create actions in this step ($item.* names). */
  draftNames: string[];
  filterOptions: IOptionItem[];
  reportOptions: IOptionItem[];
  notificationOptions: IOptionItem[];
  actionOptions: IOptionItem[];
  promptNames: string[];
  onChange: (action: IV2Action) => void;
}

/**
 * Descriptor-driven action editor: the type picker offers the action types the catalog allows in
 * this phase, and the fields render from the selected descriptor's field specs — adding an action
 * type to the engine needs no editor change here.
 */
export const V2ActionEditor: React.FC<IV2ActionEditorProps> = ({
  action,
  descriptors,
  phase,
  analysisNames,
  collectionNames,
  draftNames,
  filterOptions,
  reportOptions,
  notificationOptions,
  actionOptions,
  promptNames,
  onChange,
}) => {
  const descriptor = descriptors.find((d) => d.type === action.type);
  const typeOptions = React.useMemo(
    () =>
      descriptors
        .filter((d) => d.phases.includes(phase))
        .map((d) => createOption(`${d.category} — ${d.label}`, d.type)),
    [descriptors, phase],
  );
  const analysisOptions = analysisNames.map((name) => createOption(name, name));
  const promptOptions = promptNames.map((name) => createOption(name, name));
  const subjectOption = createOption('The subject ($item)', '$item');
  const draftOptions = draftNames.map((name) => createOption(name.replace(/^\$item\./, ''), name));
  const gate = getGate(action);

  const set = (values: Partial<IV2Action>) => onChange({ ...action, ...values });

  const renderField = (field: IV2FieldSpec) => {
    const key = `action-${field.name}`;
    switch (field.kind) {
      case 'filter':
        return (
          <V2FilterField
            key={key}
            name={key}
            label={field.name}
            value={action.filter}
            options={filterOptions}
            onChange={(filterId) => set({ filter: filterId })}
          />
        );
      case 'report':
        return (
          <Select
            key={key}
            name={key}
            label={field.name}
            width="20rem"
            options={reportOptions}
            value={findOptionByValue(reportOptions, action.report) ?? ''}
            onChange={(newValue) => set({ report: toNumberOrUndefined(newValue as IOptionItem) })}
          />
        );
      case 'notification':
        return (
          <Select
            key={key}
            name={key}
            label={field.name}
            width="20rem"
            options={notificationOptions}
            value={findOptionByValue(notificationOptions, action.notification) ?? ''}
            onChange={(newValue) =>
              set({ notification: toNumberOrUndefined(newValue as IOptionItem) })
            }
          />
        );
      case 'contentAction':
        return (
          <Select
            key={key}
            name={key}
            label="content action"
            width="20rem"
            options={actionOptions}
            value={findOptionByValue(actionOptions, action.contentAction) ?? ''}
            onChange={(newValue) =>
              set({ contentAction: toNumberOrUndefined(newValue as IOptionItem) })
            }
          />
        );
      case 'contentField':
        return (
          <Select
            key={key}
            name={key}
            label={field.name}
            width="20rem"
            options={contentFieldOptionItems}
            value={findOptionByValue(contentFieldOptionItems, action.field) ?? ''}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ field: option?.value ? `${option.value}` : null });
            }}
          />
        );
      case 'collection': {
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | string
          | undefined;
        return (
          <V2ScopedNameField
            key={key}
            name={key}
            label={`${field.name} (collection)`}
            scope="$run"
            value={current}
            knownNames={collectionNames}
            help={field.help}
            onChange={(next) => set({ [field.name]: next } as Partial<IV2Action>)}
          />
        );
      }
      case 'int': {
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | number
          | undefined;
        return (
          <Text
            key={key}
            name={key}
            label={field.name}
            width="8rem"
            type="number"
            value={current ?? ''}
            onChange={(e) =>
              set({
                [field.name]: e.target.value === '' ? null : Number(e.target.value),
              } as Partial<IV2Action>)
            }
          />
        );
      }
      case 'bool': {
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | boolean
          | undefined;
        return (
          <Checkbox
            key={key}
            name={key}
            label={field.name}
            checked={current ?? false}
            onChange={(e) => set({ [field.name]: e.target.checked } as Partial<IV2Action>)}
          />
        );
      }
      case 'condition':
        return (
          <Col key={key} className="v2-field-wide">
            <label>{field.name}</label>
            <V2ConditionBuilder
              value={action.where ?? { field: '', op: 'equals', value: '' }}
              onChange={(where) => set({ where })}
            />
          </Col>
        );
      case 'valueSource':
        return (
          <Col key={key} className="v2-field-wide">
            <label>value</label>
            <V2ValueSourceEditor
              name={key}
              value={action.value}
              onChange={(value) => set({ value })}
            />
            <Show visible={analysisNames.length > 0}>
              <p className="v2-field-help">Analyses on this step: {analysisNames.join(', ')}</p>
            </Show>
          </Col>
        );
      case 'valueMap': {
        const entries = Object.entries(action.set ?? {});
        return (
          <Col key={key} gap="0.25rem" className="v2-field-wide">
            <label>set fields</label>
            {entries.map(([fieldName, source], index) => (
              <Row key={index} gap="0.5rem" alignItems="center" nowrap>
                <Text
                  name={`${key}-field-${index}`}
                  placeholder="field"
                  width="10rem"
                  value={fieldName}
                  onChange={(e) => {
                    const updated = entries.map(([k, v], i) =>
                      i === index ? ([e.target.value, v] as const) : ([k, v] as const),
                    );
                    set({ set: Object.fromEntries(updated) });
                  }}
                />
                <V2ValueSourceEditor
                  name={`${key}-source-${index}`}
                  value={source}
                  onChange={(next) => {
                    const updated = entries.map(([k, v], i) =>
                      i === index ? ([k, next] as const) : ([k, v] as const),
                    );
                    set({ set: Object.fromEntries(updated) });
                  }}
                />
                <button
                  type="button"
                  className="rule-icon-button delete"
                  title="Remove field"
                  onClick={() => {
                    const updated = entries.filter((_, i) => i !== index);
                    set({ set: updated.length > 0 ? Object.fromEntries(updated) : null });
                  }}
                >
                  ×
                </button>
              </Row>
            ))}
            <Row>
              <button
                type="button"
                className="v2-link-button"
                onClick={() => set({ set: { ...(action.set ?? {}), '': { from: '' } } })}
              >
                + field
              </button>
            </Row>
          </Col>
        );
      }
      case 'fields': {
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | string[]
          | undefined;
        return (
          <Text
            key={key}
            name={key}
            label={`${field.name} (comma-separated)`}
            width="30rem"
            value={(current ?? []).join(', ')}
            onChange={(e) => {
              const list = e.target.value
                .split(',')
                .map((item) => item.trim())
                .filter((item) => item.length > 0);
              set({ [field.name]: list.length > 0 ? list : null } as Partial<IV2Action>);
            }}
          />
        );
      }
      case 'truncateMap':
        return (
          <Text
            key={key}
            name={key}
            label="truncate (field=chars, …)"
            placeholder="body=2000, summary=500"
            width="24rem"
            value={truncateToText(action.truncate)}
            onChange={(e) => set({ truncate: textToTruncate(e.target.value) })}
          />
        );
      case 'prompt':
        return (
          <Col key={key}>
            <Select
              name={`${key}-ref`}
              label="prompt (library entry)"
              width="20rem"
              options={promptOptions}
              value={findOptionByValue(promptOptions, action.prompt?.ref) ?? ''}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                set({
                  prompt: option?.value
                    ? { ...(action.prompt ?? {}), ref: `${option.value}` }
                    : null,
                });
              }}
            />
          </Col>
        );
      case 'item': {
        // The subject or a draft created earlier in this step - a closed set, so a dropdown.
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | string
          | undefined;
        const itemOptions = [subjectOption, ...draftOptions];
        return (
          <Select
            key={key}
            name={key}
            label={field.name}
            width="16rem"
            options={itemOptions}
            value={findOptionByValue(itemOptions, current) ?? ''}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ [field.name]: option?.value ? `${option.value}` : null } as Partial<IV2Action>);
            }}
          />
        );
      }
      case 'draft': {
        // 'as' names a NEW draft (scoped input); every other draft field targets an existing one.
        if (field.name === 'as')
          return (
            <V2ScopedNameField
              key={key}
              name={key}
              label={`${field.name} (new draft)`}
              scope="$item"
              value={action.as}
              help={field.help}
              onChange={(next) => set({ as: next })}
            />
          );
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | string
          | undefined;
        const targetOptions = [createOption('(the subject)', ''), ...draftOptions];
        return (
          <Select
            key={key}
            name={key}
            label={field.name}
            width="16rem"
            isClearable={false}
            options={targetOptions}
            value={findOptionByValue(targetOptions, current ?? '')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ [field.name]: option?.value ? `${option.value}` : null } as Partial<IV2Action>);
            }}
          />
        );
      }
      default: {
        if (field.kind.startsWith('enum:')) {
          const options = field.kind
            .slice(5)
            .split('|')
            .map((item) => createOption(item, item));
          const current = (action as unknown as Record<string, unknown>)[field.name] as
            | string
            | undefined;
          return (
            <Select
              key={key}
              name={key}
              label={field.name}
              width="12rem"
              options={options}
              value={findOptionByValue(options, current) ?? ''}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                set({
                  [field.name]: option?.value ? `${option.value}` : null,
                } as Partial<IV2Action>);
              }}
            />
          );
        }
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | string
          | undefined;
        return (
          <Text
            key={key}
            name={key}
            label={field.name}
            width="16rem"
            value={current ?? ''}
            onChange={(e) => set({ [field.name]: e.target.value || null } as Partial<IV2Action>)}
          />
        );
      }
    }
  };

  return (
    <Col className="v2-action-editor" gap="0.5rem">
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        <Select
          name="action-type"
          label="Action"
          width="24rem"
          isClearable={false}
          options={typeOptions}
          value={findOptionByValue(typeOptions, action.type)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            // Changing the type keeps only the identity/gate fields; type-specific config resets.
            if (option?.value)
              onChange({
                type: `${option.value}`,
                name: action.name,
                isEnabled: action.isEnabled,
                when: action.when,
                confirm: action.confirm,
                analysis: action.analysis,
              });
          }}
        />
        <Text
          name="action-name"
          label="Name (optional)"
          width="14rem"
          value={action.name ?? ''}
          onChange={(e) => set({ name: e.target.value || null })}
        />
        <Checkbox
          name="action-enabled"
          label="Enabled"
          checked={action.isEnabled}
          onChange={(e) => set({ isEnabled: e.target.checked })}
        />
      </Row>
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        <Select
          name="action-gate"
          label="Runs when"
          width="18rem"
          isClearable={false}
          options={gateOptions}
          value={findOptionByValue(gateOptions, gate)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            switch (option?.value) {
              case 'condition':
                set({
                  when: { field: '', op: 'equals', value: '' },
                  confirm: null,
                  analysis: null,
                });
                break;
              case 'confirm':
                set({ when: null, confirm: '[CONFIRMED]', analysis: analysisNames[0] ?? null });
                break;
              default:
                set({ when: null, confirm: null, analysis: null });
                break;
            }
          }}
        />
        <Show visible={gate === 'confirm'}>
          <Text
            name="action-confirm"
            label="Confirmation statement"
            width="18rem"
            value={action.confirm ?? ''}
            onChange={(e) => set({ confirm: e.target.value })}
          />
          <Select
            name="action-analysis"
            label="Against analysis"
            width="14rem"
            options={analysisOptions}
            value={findOptionByValue(analysisOptions, action.analysis) ?? ''}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ analysis: option?.value ? `${option.value}` : null });
            }}
          />
        </Show>
      </Row>
      <Show visible={gate === 'condition'}>
        <V2ConditionBuilder
          value={action.when ?? { field: '', op: 'equals', value: '' }}
          onChange={(when) => set({ when })}
        />
      </Show>
      <Show visible={!!descriptor}>
        <Row className="v2-action-fields" gap="0.5rem" alignItems="flex-end">
          {descriptor?.fields.map((field) => renderField(field))}
        </Row>
      </Show>
      <Show visible={!!descriptor && descriptor.fields.length === 0}>
        <p className="v2-field-help">This action needs no configuration.</p>
      </Show>
    </Col>
  );
};
