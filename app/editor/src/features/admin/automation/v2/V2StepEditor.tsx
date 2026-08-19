import React from 'react';
import { FaPlus, FaTrash } from 'react-icons/fa';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import {
  createDefaultV2Action,
  v2PhaseOptions,
  v2SourceOptions,
  v2StepSaveModeOptions,
} from './constants';
import {
  type IV2Action,
  type IV2ActionDescriptor,
  type IV2Analysis,
  type IV2Step,
} from './interfaces';
import { V2ActionEditor } from './V2ActionEditor';
import { V2AnalysisEditor } from './V2AnalysisEditor';

export interface IV2StepEditorProps {
  step: IV2Step;
  descriptors: IV2ActionDescriptor[];
  collectionNames: string[];
  promptNames: string[];
  filterOptions: IOptionItem[];
  llmOptions: IOptionItem[];
  reportOptions: IOptionItem[];
  notificationOptions: IOptionItem[];
  actionOptions: IOptionItem[];
  onChange: (step: IV2Step) => void;
}

/**
 * Editor for one v2 step: phase, content source (process steps), flush override, its named
 * analyses, and its ordered actions. Within a step every action applies to the item the step
 * iterates — to act on something else, iterate a different collection.
 */
export const V2StepEditor: React.FC<IV2StepEditorProps> = ({
  step,
  descriptors,
  collectionNames,
  promptNames,
  filterOptions,
  llmOptions,
  reportOptions,
  notificationOptions,
  actionOptions,
  onChange,
}) => {
  const set = (values: Partial<IV2Step>) => onChange({ ...step, ...values });
  const analysisNames = step.analyses.map((analysis) => analysis.name).filter((name) => !!name);
  const collectionOptions = collectionNames.map((name) => createOption(name, name));

  const setAnalysis = (index: number, analysis: IV2Analysis) => {
    const analyses = [...step.analyses];
    analyses[index] = analysis;
    set({ analyses });
  };

  const setAction = (index: number, action: IV2Action) => {
    const actions = [...step.actions];
    actions[index] = action;
    set({ actions });
  };

  const moveAction = (index: number, direction: -1 | 1) => {
    const target = index + direction;
    if (target < 0 || target >= step.actions.length) return;
    const actions = [...step.actions];
    const [moved] = actions.splice(index, 1);
    actions.splice(target, 0, moved);
    set({ actions });
  };

  return (
    <Col className="v2-step-editor" gap="0.75rem">
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        <Text
          name="step-name"
          label="Step name"
          width="16rem"
          value={step.name}
          onChange={(e) => set({ name: e.target.value })}
        />
        <Select
          name="step-phase"
          label="Phase"
          width="18rem"
          isClearable={false}
          options={v2PhaseOptions}
          value={findOptionByValue(v2PhaseOptions, step.phase)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            const phase = `${option?.value ?? 'process'}` as IV2Step['phase'];
            set({
              phase,
              // Once-phases run with no subject and cannot declare a source.
              source:
                phase === 'process'
                  ? step.source ?? { from: 'profile', include: [], exclude: [] }
                  : undefined,
            });
          }}
        />
        <Select
          name="step-save-mode"
          label="Save changes"
          width="16rem"
          isClearable={false}
          options={v2StepSaveModeOptions}
          value={findOptionByValue(v2StepSaveModeOptions, step.saveMode ?? '')}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            set({ saveMode: option?.value ? `${option.value}` : null });
          }}
        />
        <Select
          name="step-llm"
          label="LLM override"
          width="14rem"
          options={llmOptions}
          value={findOptionByValue(llmOptions, step.llmId) ?? ''}
          onChange={(newValue) => set({ llmId: toNumberOrUndefined(newValue as IOptionItem) })}
        />
        <Checkbox
          name="step-enabled"
          label="Enabled"
          checked={step.isEnabled}
          onChange={(e) => set({ isEnabled: e.target.checked })}
        />
      </Row>
      <TextArea
        name="step-description"
        label="Description"
        rows={2}
        value={step.description ?? ''}
        onChange={(e) => set({ description: e.target.value || undefined })}
      />

      <Show visible={step.phase === 'process'}>
        <Row gap="0.5rem" alignItems="flex-end" nowrap>
          <Select
            name="step-source-from"
            label="Content source"
            width="18rem"
            isClearable={false}
            options={v2SourceOptions}
            value={findOptionByValue(v2SourceOptions, step.source?.from ?? 'profile')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({
                source: {
                  ...(step.source ?? { include: [], exclude: [] }),
                  from: `${option?.value ?? 'profile'}` as NonNullable<IV2Step['source']>['from'],
                },
              });
            }}
          />
          <Show visible={step.source?.from === 'filter'}>
            <Select
              name="step-source-filter"
              label="Filter"
              width="18rem"
              options={filterOptions}
              value={findOptionByValue(filterOptions, step.source?.filter) ?? ''}
              onChange={(newValue) =>
                set({
                  source: {
                    ...(step.source ?? { from: 'filter' }),
                    from: 'filter',
                    filter: toNumberOrUndefined(newValue as IOptionItem),
                  },
                })
              }
            />
          </Show>
          <Show visible={step.source?.from === 'collection'}>
            <Select
              name="step-source-collection"
              label="Collection"
              width="16rem"
              options={collectionOptions}
              value={findOptionByValue(collectionOptions, step.source?.collection) ?? ''}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                set({
                  source: {
                    ...(step.source ?? { from: 'collection' }),
                    from: 'collection',
                    collection: option?.value ? `${option.value}` : null,
                  },
                });
              }}
            />
            <Text
              name="step-source-collection-text"
              label="or type a name"
              placeholder="$run.inbox"
              width="12rem"
              value={step.source?.collection ?? ''}
              onChange={(e) =>
                set({
                  source: {
                    ...(step.source ?? { from: 'collection' }),
                    from: 'collection',
                    collection: e.target.value || null,
                  },
                })
              }
            />
          </Show>
          <Text
            name="step-source-max"
            label="Max items"
            width="8rem"
            type="number"
            value={step.source?.max ?? ''}
            onChange={(e) =>
              set({
                source: {
                  ...(step.source ?? { from: 'profile' }),
                  max: e.target.value === '' ? null : Number(e.target.value),
                },
              })
            }
          />
        </Row>
        <GateFilterPicker
          label="Only items matching every filter (include)"
          name="step-include"
          values={step.source?.include ?? []}
          filterOptions={filterOptions}
          onChange={(include) =>
            set({ source: { ...(step.source ?? { from: 'profile' }), include } })
          }
        />
        <GateFilterPicker
          label="Skip items matching any filter (exclude)"
          name="step-exclude"
          values={step.source?.exclude ?? []}
          filterOptions={filterOptions}
          onChange={(exclude) =>
            set({ source: { ...(step.source ?? { from: 'profile' }), exclude } })
          }
        />
      </Show>

      <Row className="v2-subsection-header" nowrap>
        <h3>Analyses</h3>
        <button
          type="button"
          className="rule-icon-button"
          title="Add analysis"
          onClick={() =>
            set({
              analyses: [...step.analyses, { name: '', prompt: { text: '' }, returns: {} }],
            })
          }
        >
          <FaPlus />
        </button>
      </Row>
      <p className="v2-field-help">
        An analysis is one LLM prompt with a declared result shape. Analyses are lazy — one runs
        only when an action consumes its result. Cover several properties with one analysis to share
        a single call, or keep a complex prompt isolated in its own.
      </p>
      {step.analyses.map((analysis, index) => (
        <Row key={index} gap="0.5rem" alignItems="flex-start" nowrap className="v2-list-item">
          <V2AnalysisEditor
            analysis={analysis}
            earlierNames={step.analyses.slice(0, index).map((a) => a.name)}
            promptNames={promptNames}
            llmOptions={llmOptions}
            onChange={(next) => setAnalysis(index, next)}
          />
          <button
            type="button"
            className="rule-icon-button delete"
            title="Remove analysis"
            onClick={() => set({ analyses: step.analyses.filter((_, i) => i !== index) })}
          >
            <FaTrash />
          </button>
        </Row>
      ))}

      <Row className="v2-subsection-header" nowrap>
        <h3>Actions</h3>
        <button
          type="button"
          className="rule-icon-button"
          title="Add action"
          onClick={() =>
            set({
              actions: [
                ...step.actions,
                createDefaultV2Action(step.phase === 'process' ? 'content.update' : 'search'),
              ],
            })
          }
        >
          <FaPlus />
        </button>
      </Row>
      {step.actions.map((action, index) => (
        <Row key={index} gap="0.5rem" alignItems="flex-start" nowrap className="v2-list-item">
          <Col gap="0.25rem" className="v2-list-item-order">
            <button
              type="button"
              className="rule-icon-button"
              title="Move up"
              disabled={index === 0}
              onClick={() => moveAction(index, -1)}
            >
              ↑
            </button>
            <button
              type="button"
              className="rule-icon-button"
              title="Move down"
              disabled={index === step.actions.length - 1}
              onClick={() => moveAction(index, 1)}
            >
              ↓
            </button>
          </Col>
          <V2ActionEditor
            action={action}
            descriptors={descriptors}
            phase={step.phase}
            analysisNames={analysisNames}
            collectionNames={collectionNames}
            filterOptions={filterOptions}
            reportOptions={reportOptions}
            notificationOptions={notificationOptions}
            actionOptions={actionOptions}
            promptNames={promptNames}
            onChange={(next) => setAction(index, next)}
          />
          <button
            type="button"
            className="rule-icon-button delete"
            title="Remove action"
            onClick={() => set({ actions: step.actions.filter((_, i) => i !== index) })}
          >
            <FaTrash />
          </button>
        </Row>
      ))}
    </Col>
  );
};

interface IGateFilterPickerProps {
  label: string;
  name: string;
  values: number[];
  filterOptions: IOptionItem[];
  onChange: (values: number[]) => void;
}

/** Chip-list picker for gate filter ids (each resolves once per run to an id set). */
const GateFilterPicker: React.FC<IGateFilterPickerProps> = ({
  label,
  name,
  values,
  filterOptions,
  onChange,
}) => (
  <Col gap="0.25rem">
    <Row gap="0.5rem" alignItems="flex-end" nowrap>
      <Select
        name={name}
        label={label}
        width="20rem"
        placeholder="add a filter…"
        options={filterOptions.filter((option) => !values.includes(Number(option.value)))}
        value={''}
        onChange={(newValue) => {
          const id = toNumberOrUndefined(newValue as IOptionItem);
          if (id !== undefined && !values.includes(id)) onChange([...values, id]);
        }}
      />
    </Row>
    <Show visible={values.length > 0}>
      <Row gap="0.25rem" className="v2-chips">
        {values.map((id) => (
          <span key={id} className="v2-chip">
            {findOptionByValue(filterOptions, id)?.label ?? `filter ${id}`}
            <button
              type="button"
              title="Remove"
              onClick={() => onChange(values.filter((value) => value !== id))}
            >
              ×
            </button>
          </span>
        ))}
      </Row>
    </Show>
  </Col>
);
