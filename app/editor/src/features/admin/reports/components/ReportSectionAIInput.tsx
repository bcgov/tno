import React from 'react';
import {
  Col,
  type IReportSectionModel,
  type IReportSectionSettingsModel,
  OptionItem,
  ReportSectionTypeName,
  Row,
  Select,
  Show,
} from 'tno-core';

const scopeOptions = [
  new OptionItem('Every content section in the report', 'Report'),
  new OptionItem('Selected sections', 'Sections'),
];

const outputOptions = [
  new OptionItem('Written from the prompt', 'FreeText'),
  new OptionItem('Topic summary (headings, bullets, sources)', 'TopicSummary'),
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
        <Select
          name="aiOutputMode"
          label="Output"
          tooltip="Written from the prompt, or a topic summary built from the stories with a source list for every statement."
          width="36ch"
          isClearable={false}
          options={outputOptions}
          value={outputOptions.find((o) => o.value === (settings.aiOutputMode ?? 'FreeText'))}
          onChange={(o) => onChange('aiOutputMode', (o as OptionItem)?.value ?? 'FreeText')}
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
    </Col>
  );
};
