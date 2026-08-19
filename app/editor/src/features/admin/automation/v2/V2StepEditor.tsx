import React from 'react';
import { Draggable } from 'react-beautiful-dnd';
import { FaGripLines, FaPlus, FaTrash } from 'react-icons/fa';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { StrictModeDroppable } from '../StrictModeDroppable';
import { findOptionByValue, toNumberOrUndefined } from '../utils';
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
import { V2FilterField } from './V2FilterField';
import { V2ScopedNameField } from './V2ScopedNameField';

export interface IV2StepEditorProps {
  step: IV2Step;
  /** The step's position; keys this step's actions droppable in the shared drag context. */
  stepIndex: number;
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
  stepIndex,
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
  const DroppableAny = StrictModeDroppable as any;
  const DraggableAny = Draggable as any;
  const analysisNames = step.analyses.map((analysis) => analysis.name).filter((name) => !!name);

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

  return (
    <Col className="v2-step-editor" gap="0.5rem">
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
            <V2FilterField
              name="step-source-filter"
              value={step.source?.filter}
              options={filterOptions}
              onChange={(filterId) =>
                set({
                  source: {
                    ...(step.source ?? { from: 'filter' }),
                    from: 'filter',
                    filter: filterId,
                  },
                })
              }
            />
          </Show>
          <Show visible={step.source?.from === 'collection'}>
            <V2ScopedNameField
              name="step-source-collection"
              label="Collection"
              scope="$run"
              value={step.source?.collection}
              knownNames={collectionNames}
              onChange={(next) =>
                set({
                  source: {
                    ...(step.source ?? { from: 'collection' }),
                    from: 'collection',
                    collection: next,
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
      <DroppableAny droppableId={`v2-actions-${stepIndex}`}>
        {(provided: any) => (
          <div className="v2-actions-list" ref={provided.innerRef} {...provided.droppableProps}>
            {step.actions.map((action, index) => (
              <DraggableAny
                key={`action-${stepIndex}-${index}`}
                draggableId={`action-${stepIndex}-${index}`}
                index={index}
              >
                {(dragProvided: any, dragSnapshot: any) => (
                  <div
                    className={`v2-list-item${dragSnapshot.isDragging ? ' is-dragging' : ''}`}
                    ref={dragProvided.innerRef}
                    {...dragProvided.draggableProps}
                  >
                    <Row gap="0.5rem" alignItems="flex-start" nowrap>
                      <span
                        className="v2-drag-handle"
                        title="Drag to reorder"
                        {...dragProvided.dragHandleProps}
                      >
                        <FaGripLines />
                      </span>
                      <V2ActionEditor
                        action={action}
                        descriptors={descriptors}
                        phase={step.phase}
                        analysisNames={analysisNames}
                        collectionNames={collectionNames}
                        draftNames={step.actions
                          .slice(0, index)
                          .filter((earlier) => earlier.type === 'content.create' && !!earlier.as)
                          .map((earlier) => earlier.as!)}
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
                  </div>
                )}
              </DraggableAny>
            ))}
            {provided.placeholder}
          </div>
        )}
      </DroppableAny>
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

/** Chip-list picker for gate filter ids (each resolves once per run to an id set). Chips open
 * their filter in a new tab; the plus opens a blank filter form in a new tab. */
const GateFilterPicker: React.FC<IGateFilterPickerProps> = ({
  label,
  name,
  values,
  filterOptions,
  onChange,
}) => (
  <Col gap="0.25rem">
    <Row gap="0.25rem" alignItems="flex-end" nowrap>
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
      <button
        type="button"
        className="rule-icon-button"
        aria-label="Create a new filter in a new tab"
        title="Create a new filter in a new tab"
        onClick={() => window.open('/admin/filters/0', '_blank', 'noopener')}
      >
        <FaPlus />
      </button>
    </Row>
    <Show visible={values.length > 0}>
      <Row gap="0.25rem" className="v2-chips">
        {values.map((id) => (
          <span key={id} className="v2-chip">
            <button
              type="button"
              className="v2-chip-open"
              title="Edit this filter in a new tab"
              onClick={() => window.open(`/admin/filters/${id}`, '_blank', 'noopener')}
            >
              {findOptionByValue(filterOptions, id)?.label ?? `filter ${id}`}
            </button>
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
