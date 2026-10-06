import React from 'react';
import {
  Checkbox,
  Col,
  type IReportSectionModel,
  type IReportSectionSettingsModel,
  OptionItem,
  ReportSectionTypeName,
  Row,
  Select,
  Show,
} from 'tno-core';

import { aiInputFields, defaultAIInputFields } from './aiInputFields';

const scopeOptions = [
  new OptionItem('Every content section in the report', 'Report'),
  new OptionItem('Selected sections', 'Sections'),
];

export interface IReportSectionAIInputProps {
  /** The AI section's settings. */
  settings: IReportSectionSettingsModel;
  /** Every section of the report. */
  sections: IReportSectionModel[];
  /** Change one setting of the AI section. */
  onChange: (name: keyof IReportSectionSettingsModel, value: any) => void;
}

/**
 * Chooses the content an AI section reads and what it produces. Every story in the chosen sections
 * is processed; large reports are summarized in bounded steps.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportSectionAIInput: React.FC<IReportSectionAIInputProps> = ({
  settings,
  sections,
  onChange,
}) => {
  const fieldId = React.useId();
  const selectedFields = settings.aiInputFields ?? defaultAIInputFields;
  const scope = settings.aiScope ?? 'Report';
  const sectionOptions = sections
    .filter((s) => s.sectionType === ReportSectionTypeName.Content)
    .map((s) => new OptionItem(s.settings.label || s.description || 'Untitled section', s.name));
  const selected = settings.sourceSections ?? [];

  return (
    <Col gap="0.5rem">
      <Row gap="1rem" alignItems="flex-start">
        <Select
          name="aiScope"
          label="Stories read"
          tooltip="Which content sections this AI section reads. Every story in them is included."
          width="28ch"
          isClearable={false}
          options={scopeOptions}
          value={scopeOptions.find((o) => o.value === scope)}
          onChange={(o) => onChange('aiScope', (o as OptionItem)?.value ?? 'Report')}
        />
      </Row>
      <Show visible={scope === 'Sections'}>
        <Select
          name="sourceSections"
          label="Sections"
          tooltip="The content sections this AI section reads"
          isMulti
          options={sectionOptions}
          value={sectionOptions.filter((o) => selected.includes(o.value as string))}
          onChange={(o) =>
            onChange(
              'sourceSections',
              ((o as OptionItem[]) ?? []).map((i) => i.value as string),
            )
          }
          error={!selected.length ? 'Choose at least one section' : undefined}
        />
      </Show>
      <fieldset>
        <legend>Data fields sent to AI</legend>
        <p>
          Select only the fields needed by your prompt to reduce the amount of data sent. These
          choices also apply to prior reports. Story references for links are always retained.
          Article text takes priority over Summary when both are selected. If article text is empty,
          the selected summary is used. When unselected, article text is still used if no summary is
          available. Summary and article text are never sent together.
        </p>
        <Row gap="1rem">
          {aiInputFields.map((field) => (
            <Checkbox
              key={field.value}
              id={`${fieldId}-${field.value}`}
              role="checkbox"
              name={`aiInputFields-${field.value}`}
              label={field.label}
              tooltip={field.description}
              checked={selectedFields.includes(field.value)}
              onChange={(event) =>
                onChange(
                  'aiInputFields',
                  event.target.checked
                    ? [...selectedFields, field.value]
                    : selectedFields.filter((value) => value !== field.value),
                )
              }
            />
          ))}
        </Row>
        <Show visible={!selectedFields.length}>
          <p role="alert">Choose at least one data field before generating this section.</p>
        </Show>
        <button type="button" onClick={() => onChange('aiInputFields', [...defaultAIInputFields])}>
          Restore default fields
        </button>
      </fieldset>
    </Col>
  );
};
