import React from 'react';
import { FaPlus } from 'react-icons/fa';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { findOptionByValue, toNumberOrUndefined } from '../utils';
import { v2PhaseOptions, v2SourceOptions, v2StepSaveModeOptions } from './constants';
import { type IV2Step } from './interfaces';
import { V2FilterField } from './V2FilterField';
import { V2ScopedNameField } from './V2ScopedNameField';

export interface IV2StepEditorProps {
  step: IV2Step;
  collectionNames: string[];
  filterOptions: IOptionItem[];
  llmOptions: IOptionItem[];
  onChange: (step: IV2Step) => void;
}

/**
 * The step settings form (shown in the step modal): phase, content source with gate filters
 * (process steps), flush override, and LLM override. The step's analyses and actions are managed
 * from the steps grid, not here.
 */
export const V2StepEditor: React.FC<IV2StepEditorProps> = ({
  step,
  collectionNames,
  filterOptions,
  llmOptions,
  onChange,
}) => {
  const set = (values: Partial<IV2Step>) => onChange({ ...step, ...values });

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
        <Checkbox
          name="step-enabled"
          label="Enabled"
          checked={step.isEnabled}
          onChange={(e) => set({ isEnabled: e.target.checked })}
        />
      </Row>
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
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
