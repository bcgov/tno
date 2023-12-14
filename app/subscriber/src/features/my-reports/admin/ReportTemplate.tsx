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
  FaAngleDown,
  FaChartPie,
  FaGripLines,
  FaList,
  FaMinus,
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
  Text,
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

  const [show, setShow] = React.useState(true);
  const [sortOrders, setSortOrders] = React.useState<number[]>(
    values.sections.map((s) => s.sortOrder),
  );

  React.useEffect(() => {
    setSortOrders(values.sections.map((s) => s.sortOrder));
  }, [values.sections]);

  const addSection = React.useCallback(
    (
      index: number,
      type: ReportSectionTypeName,
      showCharts: boolean = false,
      showHeadlines: boolean | undefined = undefined,
      showFullStory: boolean | undefined = undefined,
    ) => {
      const newSection = defaultReportSection(
        type,
        index === 0
          ? 0
          : index < values.sections.length
          ? values.sections[index].sortOrder + 1
          : values.sections[values.sections.length - 1].sortOrder,
        showCharts,
        showHeadlines,
        showFullStory,
        values.hideEmptySections,
      );
      const sections = [...values.sections];
      sections.splice(index, 0, newSection);
      setFieldValue(
        'sections',
        sections.map((s, i) => ({ ...s, sortOrder: i })),
      );
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
          onClick={() => addSection(0, ReportSectionTypeName.TableOfContents)}
          disabled={
            isSubmitting ||
            values.sections.some(
              (s) => s.settings.sectionType === ReportSectionTypeName.TableOfContents,
            )
          }
        >
          <Row gap="1rem">
            <FaList />
            <label>Table of Contents</label>
          </Row>
        </Button>
        <Button
          onClick={() => addSection(values.sections.length, ReportSectionTypeName.Summary)}
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaAlignJustify />
            <label>Text</label>
          </Row>
        </Button>
        <Button
          onClick={() =>
            addSection(values.sections.length, ReportSectionTypeName.Content, false, false, true)
          }
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaNewspaper />
            <label>Media Stories</label>
          </Row>
        </Button>
        <Button
          onClick={() =>
            addSection(values.sections.length, ReportSectionTypeName.Content, true, false, false)
          }
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaChartPie />
            <label>Media Analytics</label>
          </Row>
        </Button>
      </Row>
      <Row className="template-action-bar">
        {!!values.sections.length &&
          (values.sections.some((s) => s.open) ? (
            <Action
              icon={<FaMinus />}
              label="Close All"
              onClick={() => {
                setShow(false);
                setFieldValue(
                  `sections`,
                  values.sections.map((s) => ({ ...s, open: false })),
                );
              }}
            />
          ) : (
            <Action
              icon={<FaAngleDown />}
              label="Open All"
              onClick={() => {
                setShow(true);
                setFieldValue(
                  'sections',
                  values.sections.map((s) => ({ ...s, open: true })),
                );
              }}
            />
          ))}
      </Row>
      <Col className="report-template">
        <Section label="Report Name / Description" open={show} onChange={(open) => setShow(open)}>
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
                            open={section.open}
                            onChange={(open) => setFieldValue(`sections.${index}.open`, open)}
                            actions={
                              <Row gap="1rem">
                                <Text
                                  name={`sections.${index}.sortOrder`}
                                  value={sortOrders.length > index ? sortOrders[index] : ''}
                                  width="6ch"
                                  className="align-right"
                                  onChange={(e) => {
                                    setSortOrders(
                                      values.sections.map((s, i) =>
                                        i === index ? +e.target.value : s.sortOrder,
                                      ),
                                    );
                                  }}
                                  onBlur={(e) => {
                                    onChange?.({
                                      ...values,
                                      sections: values.sections
                                        .map((s, i) =>
                                          i === index ? { ...s, sortOrder: +e.target.value } : s,
                                        )
                                        .sort((a, b) =>
                                          a.sortOrder < b.sortOrder
                                            ? -1
                                            : a.sortOrder > b.sortOrder
                                            ? 1
                                            : 0,
                                        ),
                                    });
                                    // setFieldValue(`sections.${index}.sortOrder`, +e.target.value);
                                  }}
                                />
                                <FormikCheckbox
                                  name={`sections.${index}.isEnabled`}
                                  label="Enabled"
                                />
                                <Action>
                                  <FaTrash onClick={() => removeSection(index)} />
                                </Action>
                              </Row>
                            }
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
