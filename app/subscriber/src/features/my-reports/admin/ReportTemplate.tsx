import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import { useFormikContext } from 'formik';
import React from 'react';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
  ResponderProvided,
} from 'react-beautiful-dnd';
import {
  FaAlignJustify,
  FaChartPie,
  FaGripLines,
  FaList,
  FaNewspaper,
  FaPlus,
  FaTrash,
} from 'react-icons/fa6';
import {
  Col,
  FormikCheckbox,
  FormikText,
  FormikTextArea,
  ReportSectionTypeName,
  Row,
  Show,
} from 'tno-core';

import { defaultReportSection } from '../constants';
import { IReportForm } from '../interfaces';
import { getBlockName } from '../utils';
import { ReportSectionContent } from './old/components/ReportSectionContent';
import { ReportSectionSummary } from './old/components/ReportSectionSummary';
import { ReportSectionTableOfContents } from './old/components/ReportSectionTableOfContents';

export interface IReportTemplateProps {
  onChange?: (values: IReportForm) => void;
}

export const ReportTemplate: React.FC<IReportTemplateProps> = ({ onChange }) => {
  const { setFieldValue, values, isSubmitting } = useFormikContext<IReportForm>();

  const addSection = React.useCallback(
    (
      type: ReportSectionTypeName,
      showCharts: boolean = false,
      showHeadlines: boolean | undefined = undefined,
      showFullStory: boolean | undefined = undefined,
    ) => {
      const sections = [
        ...values.sections,
        defaultReportSection(
          type,
          values.sections.length,
          showCharts,
          showHeadlines,
          showFullStory,
          values.hideEmptySections,
        ),
      ];
      setFieldValue('sections', sections);
    },
    [setFieldValue, values],
  );

  const removeSection = React.useCallback(
    (index: number) => {
      setFieldValue(
        'sections',
        values.sections
          .filter((s, i) => i !== index)
          .map((section, index) => ({
            ...section,
            sortOrder: index,
          })),
      );
    },
    [setFieldValue, values.sections],
  );

  /** function that runs after a user drops an item in the list */
  const handleDrop = React.useCallback(
    (result: DropResult, provided: ResponderProvided) => {
      if (!result.destination) {
        return;
      }
      var sections = [...values.sections];
      const [reorderedItem] = sections.splice(result.source.index, 1);
      sections.splice(result.destination.index, 0, reorderedItem);
      const reorderedSections = sections.map((section, index) => ({
        ...section,
        sortOrder: index,
      }));
      // Update State
      setFieldValue(`sections`, reorderedSections);
    },
    [setFieldValue, values.sections],
  );

  return (
    <Col>
      <Row className="section-bar" gap="1rem" justifyContent="center">
        <FaPlus />
        <Button
          onClick={() => addSection(ReportSectionTypeName.TableOfContents)}
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaList />
            <label>Table of Contents</label>
          </Row>
        </Button>
        <Button onClick={() => addSection(ReportSectionTypeName.Summary)} disabled={isSubmitting}>
          <Row gap="1rem">
            <FaAlignJustify />
            <label>Text</label>
          </Row>
        </Button>
        <Button
          onClick={() => addSection(ReportSectionTypeName.Content, false, false, true)}
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaNewspaper />
            <label>Media Stories</label>
          </Row>
        </Button>
        <Button
          onClick={() => addSection(ReportSectionTypeName.Content, true, false, false)}
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaChartPie />
            <label>Media Analytics</label>
          </Row>
        </Button>
      </Row>
      <Col className="report-template">
        <Section label="Report Name / Description" open={true}>
          <p>Name your report and provide a description that will help you identify it.</p>
          <FormikText
            name="name"
            label="Report Name:"
            required
            placeholder="Enter unique report name"
            onChange={(e) => {
              setFieldValue('name', e.target.value);
              onChange?.({ ...values, name: e.target.value });
            }}
          />
          <FormikTextArea name="description" label="Description:" />
        </Section>
        <DragDropContext onDragEnd={handleDrop}>
          <Droppable droppableId="report-sections-container">
            {(provided) => (
              <div {...provided.droppableProps} ref={provided.innerRef}>
                {values.sections.map((section, index) => {
                  return (
                    <Draggable key={index} draggableId={index.toString()} index={index}>
                      {(provided) => (
                        <div
                          key={`${section.id}-${index}`}
                          className="section-block"
                          ref={provided.innerRef}
                          {...provided.dragHandleProps}
                          {...provided.draggableProps}
                        >
                          <Section
                            icon={<FaGripLines />}
                            label={getBlockName(section)}
                            actions={
                              <Row gap="1rem">
                                <FormikCheckbox
                                  name={`sections.${index}.isEnabled`}
                                  label="Enabled"
                                />
                                <Action>
                                  <FaTrash onClick={() => removeSection(index)} />
                                </Action>
                              </Row>
                            }
                            open={true}
                          >
                            <Show
                              visible={
                                section.settings.sectionType ===
                                ReportSectionTypeName.TableOfContents
                              }
                            >
                              <ReportSectionTableOfContents index={index} />
                            </Show>
                            <Show
                              visible={
                                section.settings.sectionType === ReportSectionTypeName.Content
                              }
                            >
                              <ReportSectionContent index={index} />
                            </Show>
                            <Show
                              visible={
                                section.settings.sectionType === ReportSectionTypeName.Summary
                              }
                            >
                              <ReportSectionSummary index={index} />
                            </Show>
                          </Section>
                        </div>
                      )}
                    </Draggable>
                  );
                })}
              </div>
            )}
          </Droppable>
          <div></div>
        </DragDropContext>
      </Col>
    </Col>
  );
};
