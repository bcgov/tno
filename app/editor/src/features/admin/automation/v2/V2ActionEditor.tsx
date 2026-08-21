import React from 'react';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text } from 'tno-core';

import { contentFieldOptionItems } from '../constants';
import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import { fitSelectWidth, v2ContentFieldOptions, v2CopyFieldOptions } from './constants';
import {
  type IV2Action,
  type IV2ActionDescriptor,
  type IV2Analysis,
  type IV2FieldSpec,
} from './interfaces';
import { V2ComboBox } from './V2ComboBox';
import { V2ConditionBuilder } from './V2ConditionBuilder';
import { V2DraftText } from './V2DraftText';
import { V2FieldsPicker } from './V2FieldsPicker';
import { V2FilterField } from './V2FilterField';
import { V2ScopedNameField } from './V2ScopedNameField';
import { kindHelp, V2ValueSourceEditor } from './V2ValueSourceEditor';

const gateOptions: IOptionItem[] = [
  createOption('Always run', 'always'),
  createOption('Condition', 'condition'),
  createOption('LLM confirmation statement', 'confirm'),
];

/** The gate, with dedupe-result shapes recognised so the friendly options display as chosen:
 * when = {from: 'name.isDuplicate'} -> dupe:<ref>; when = {not: {from: ...}} -> unique:<ref>. */
const getGate = (action: IV2Action, dedupeRefs: string[]): string => {
  if (action.confirm) return 'confirm';
  if (action.when) {
    if (action.when.from != null && dedupeRefs.includes(action.when.from))
      return `dupe:${action.when.from}`;
    const notFrom = action.when.not?.from;
    if (notFrom != null && dedupeRefs.includes(notFrom)) return `unique:${notFrom}`;
    return 'condition';
  }
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
  /** The step's analyses; drives the confirm picker and the bool-answer autocomplete. */
  analyses: IV2Analysis[];
  /** '<name>.isDuplicate' references published by the step's Detect Duplicate actions. */
  dedupeRefs?: string[];
  /** Objectives recorded by score actions anywhere in the definition (for select-top). */
  objectiveNames?: string[];
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
  analyses,
  dedupeRefs = [],
  objectiveNames = [],
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
  // Grouped by category for the menu; a flat list backs the value lookup.
  const typeGroups = React.useMemo(() => {
    const allowed = descriptors.filter((d) => d.phases.includes(phase));
    const categories = Array.from(new Set(allowed.map((d) => d.category))).sort();
    return categories.map((category) => ({
      label: category,
      options: allowed
        .filter((d) => d.category === category)
        .map((d) => createOption(d.label, d.type))
        .sort((a, b) => `${a.label}`.localeCompare(`${b.label}`)),
    }));
  }, [descriptors, phase]);
  const typeOptions = React.useMemo(
    () => typeGroups.flatMap((group) => group.options),
    [typeGroups],
  );
  const analysisNames = analyses.map((analysis) => analysis.name).filter((name) => !!name);
  const analysisOptions = analysisNames.map((name) => createOption(name, name));
  // 'analysisName.key' references whose declared return type is bool.
  const analysisBoolRefs = analyses
    .filter((analysis) => !analysis.raw)
    .flatMap((analysis) =>
      Object.entries(analysis.returns ?? {})
        .filter(([, type]) => `${type}`.trim().toLowerCase().startsWith('bool'))
        .map(([key]) => `${analysis.name}.${key}`),
    );
  const promptOptions = promptNames.map((name) => createOption(name, name));
  // Everything a value source can read: declared analysis keys, dedupe results, and the
  // working-copy fields.
  const valueRefs = [
    ...analyses
      .filter((analysis) => !analysis.raw)
      .flatMap((analysis) =>
        Object.keys(analysis.returns ?? {}).map((key) => `${analysis.name}.${key}`),
      ),
    ...dedupeRefs.flatMap((ref) => [ref, ref.replace(/\.isDuplicate$/, '.matchedId')]),
    ...v2ContentFieldOptions.map((option) => `content.${option.value}`),
  ];
  const subjectOption = createOption('original item', '$item');
  const draftOptions = draftNames.map((name) => createOption(name.replace(/^\$item\./, ''), name));
  const gate = getGate(action, dedupeRefs);
  // Every Detect Duplicate in the step contributes ready-made routing gates, so the connection
  // between the detector and the routed action is a visible choice, not a recipe.
  const dedupeGateOptions = dedupeRefs.flatMap((ref) => {
    const dedupeName = ref.replace(/\.isDuplicate$/, '');
    return [
      createOption(`'${dedupeName}' found a duplicate`, `dupe:${ref}`),
      createOption(`'${dedupeName}' found no duplicate`, `unique:${ref}`),
    ];
  });
  const allGateOptions = [...gateOptions, ...dedupeGateOptions];

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
          <div key={key} className="checkbox-inline">
            <Checkbox
              name={key}
              label={field.name}
              checked={current ?? false}
              onChange={(e) => set({ [field.name]: e.target.checked } as Partial<IV2Action>)}
            />
          </div>
        );
      }
      case 'condition':
        return (
          <div key={key} className="v2-field-wide frm-in">
            <label>{field.name}</label>
            <V2ConditionBuilder
              value={action.where ?? { field: '', op: 'equals', value: '' }}
              onChange={(where) => set({ where })}
            />
          </div>
        );
      case 'valueSource':
        return (
          <div key={key} className="v2-field-wide frm-in">
            <label>value</label>
            <V2ValueSourceEditor
              name={key}
              value={action.value}
              fromSuggestions={valueRefs}
              onChange={(value) => set({ value })}
            />
            <Show visible={analysisNames.length > 0}>
              <p className="v2-field-help">Analyses on this step: {analysisNames.join(', ')}</p>
            </Show>
          </div>
        );
      case 'valueMap': {
        const entries = Object.entries(action.set ?? {});
        return (
          <div key={key} className="v2-field-wide frm-in">
            <label>set fields</label>
            <p className="v2-field-help">
              Each row sets one field on every item. Analysis result / content field:{' '}
              {kindHelp.from} Literal value: {kindHelp.literal} Template: {kindHelp.template}
            </p>
            <Col gap="0.25rem">
              {entries.map(([fieldName, source], index) => (
                <Row
                  key={index}
                  gap="0.5rem"
                  alignItems="flex-start"
                  nowrap
                  className="v2-set-fields-row"
                >
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
                    fromSuggestions={valueRefs}
                    showHelp={false}
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
          </div>
        );
      }
      case 'fields': {
        const current = (action as unknown as Record<string, unknown>)[field.name] as
          | string[]
          | undefined;
        // 'copyFields' -> 'Copy fields'.
        const fieldsLabel = field.name
          .replace(/([A-Z])/g, ' $1')
          .toLowerCase()
          .replace(/^./, (c) => c.toUpperCase());
        // Copy fields offers an 'all fields' checkbox, stored as the '*' sentinel the engine
        // expands to every field the item carries.
        const offersAll = field.name === 'copyFields';
        const allFields = offersAll && (current ?? []).includes('*');
        return (
          <Row key={key} gap="1rem" alignItems="flex-start" nowrap className="v2-copy-fields-row">
            <Show visible={!allFields}>
              <V2FieldsPicker
                name={key}
                label={fieldsLabel}
                values={current ?? []}
                suggestions={v2CopyFieldOptions.map((option) => `${option.value}`)}
                onChange={(list) =>
                  set({ [field.name]: list.length > 0 ? list : null } as Partial<IV2Action>)
                }
              />
            </Show>
            <Show visible={allFields}>
              <div className="frm-in">
                <label>{fieldsLabel}</label>
                <p className="v2-field-help">Every field the item carries is copied.</p>
              </div>
            </Show>
            <Show visible={offersAll}>
              <div className="checkbox-inline">
                <Checkbox
                  name={`${key}-all`}
                  label="all fields"
                  checked={allFields}
                  onChange={(e) =>
                    set({ [field.name]: e.target.checked ? ['*'] : null } as Partial<IV2Action>)
                  }
                />
              </div>
            </Show>
          </Row>
        );
      }
      case 'truncateMap':
        return (
          <V2DraftText
            key={key}
            name={key}
            label="truncate (field=chars, …)"
            placeholder="body=2000, summary=500"
            width="24rem"
            canonical={truncateToText(action.truncate)}
            onText={(text) => set({ truncate: textToTruncate(text) })}
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
        // With no drafts in the step the only possible value is the item being processed -
        // the field would be a one-option dropdown, so it only renders when a choice exists.
        if (draftNames.length === 0) return null;
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
              set({
                [field.name]: option?.value ? `${option.value}` : null,
              } as Partial<IV2Action>);
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
        if (draftNames.length === 0) return null;
        const targetOptions = [createOption('(original item)', ''), ...draftOptions];
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
              set({
                [field.name]: option?.value ? `${option.value}` : null,
              } as Partial<IV2Action>);
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
        // Objectives are declared by score actions; offer them (typing a new one is how a
        // score action declares it in the first place).
        if (field.name === 'objective')
          return (
            <div key={key} className="frm-in">
              <label>{field.name}</label>
              <V2ComboBox
                name={key}
                placeholder={action.type === 'score' ? 'Name an objective' : 'Pick an objective'}
                width="16rem"
                suggestions={objectiveNames}
                value={current ?? ''}
                onChange={(next) => set({ [field.name]: next || null } as Partial<IV2Action>)}
              />
            </div>
          );
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
      <Row gap="1rem" alignItems="flex-end" nowrap>
        <Text
          name="action-name"
          label="Name"
          placeholder="Optional name"
          width="14rem"
          value={action.name ?? ''}
          onChange={(e) => set({ name: e.target.value || null })}
        />
        <div className="checkbox-inline">
          <Checkbox
            name="action-enabled"
            label="Enabled"
            checked={action.isEnabled}
            onChange={(e) => set({ isEnabled: e.target.checked })}
          />
        </div>
      </Row>
      <Row gap="1rem" alignItems="flex-end" nowrap>
        <Select
          name="action-type"
          label="Action"
          required
          width="18rem"
          isClearable={false}
          placeholder="Pick an action"
          options={typeGroups}
          value={findOptionByValue(typeOptions, action.type) ?? ''}
          formatOptionLabel={(data, meta) => {
            const option = data as IOptionItem;
            // The control shows 'category > label'; menu rows show the label under their
            // group heading.
            if (meta.context === 'value') {
              const selected = descriptors.find((d) => d.type === option.value);
              return selected ? `${selected.category} › ${selected.label}` : option.label;
            }
            return option.label;
          }}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            // The type controls which fields exist and are required, so changing it resets the
            // action entirely - only the name and enabled flag carry over.
            if (option?.value)
              onChange({
                type: `${option.value}`,
                name: action.name,
                isEnabled: action.isEnabled,
              });
          }}
        />
        <Select
          name="action-gate"
          label="Runs when"
          required
          width={fitSelectWidth(allGateOptions.map((option) => `${option.label}`))}
          isClearable={false}
          options={allGateOptions}
          value={findOptionByValue(allGateOptions, gate)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            const value = `${option?.value ?? ''}`;
            if (value.startsWith('dupe:')) {
              set({ when: { from: value.slice(5) }, confirm: null, analysis: null });
              return;
            }
            if (value.startsWith('unique:')) {
              set({ when: { not: { from: value.slice(7) } }, confirm: null, analysis: null });
              return;
            }
            switch (value) {
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
      </Row>
      <Show visible={gate === 'confirm'}>
        <Row gap="1rem" alignItems="flex-end" nowrap>
          <Text
            name="action-confirm"
            label="Confirmation statement"
            width="16rem"
            value={action.confirm ?? ''}
            onChange={(e) => set({ confirm: e.target.value })}
          />
          <Select
            name="action-analysis"
            label="Against analysis"
            width={fitSelectWidth(analysisNames, '', 4, 18)}
            options={analysisOptions}
            value={findOptionByValue(analysisOptions, action.analysis) ?? ''}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ analysis: option?.value ? `${option.value}` : null });
            }}
          />
        </Row>
      </Show>
      <Show visible={gate === 'condition'}>
        <div className="frm-in">
          <label>Condition</label>
          <V2ConditionBuilder
            value={action.when ?? { field: '', op: 'equals', value: '' }}
            fromSuggestions={[...analysisBoolRefs, ...dedupeRefs]}
            onChange={(when) => set({ when })}
          />
          <p className="v2-config-hint">
            Evaluated against the working copy. When it fails, no prompt is sent.
          </p>
        </div>
      </Show>
      <label className="v2-config-label">Configuration</label>
      <Show visible={!descriptor}>
        <p className="v2-config-hint">Pick an action to configure it.</p>
      </Show>
      <Show visible={!!descriptor?.description}>
        <p className="v2-config-desc">{descriptor?.description}</p>
      </Show>
      <Show visible={!!descriptor}>
        <Row className="v2-action-fields" gap="1rem" alignItems="flex-start">
          {descriptor?.fields.map((field) => renderField(field))}
        </Row>
      </Show>
      <Show visible={!!descriptor && descriptor.fields.length === 0}>
        <p className="v2-field-help">This action needs no configuration.</p>
      </Show>
    </Col>
  );
};
