import { Action } from 'components/action';
import { Section } from 'components/section';
import { SectionLabel } from 'features/my-reports/components';
import React from 'react';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
  ResponderProvided,
} from 'react-beautiful-dnd';
import { FaAngleDown, FaGripVertical, FaMinus, FaTrash } from 'react-icons/fa6';
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

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';
import {
  AddSectionBar,
  ReportSectionContent,
  ReportSectionGallery,
  ReportSectionMediaAnalytics,
  ReportSectionTableOfContents,
  ReportSectionText,
} from './template';

export const ReportEditTemplateForm = () => {
  const { values, setFieldValue, setValues } = useReportEditContext();

  const [show, setShow] = React.useState(true);
  const [sortOrders, setSortOrders] = React.useState<number[]>(
    values.sections.map((s) => s.sortOrder),
  );

  React.useEffect(() => {
    setSortOrders(values.sections.map((s) => s.sortOrder));
  }, [values.sections]);

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
    <styled.ReportEditTemplateForm>
      <AddSectionBar />
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
      <Col className="report-template report-edit-section">
        <Section label="Report Name / Description" open={show} onChange={(open) => setShow(open)}>
          <p>Name your report and provide a description that will help you identify it.</p>
          <FormikText
            name="name"
            label="Report Name:"
            required
            placeholder="Enter unique report name"
            onChange={(e) => {
              setFieldValue('name', e.target.value);
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
                            icon={<FaGripVertical className="grip-bar" />}
                            label={<SectionLabel section={section} />}
                            open={section.open}
                            className="template-section"
                            onChange={(open) => setFieldValue(`sections.${index}.open`, open)}
                            actions={
                              <Row gap="1rem" alignItems="center">
                                <Text
                                  name={`sections.${index}.sortOrder`}
                                  value={sortOrders.length > index ? sortOrders[index] : ''}
                                  width="5ch"
                                  className="section-sort"
                                  onChange={(e) => {
                                    setSortOrders(
                                      values.sections.map((s, i) =>
                                        i === index ? +e.target.value : s.sortOrder,
                                      ),
                                    );
                                  }}
                                  onBlur={(e) => {
                                    setValues({
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
                            {/* TABLE OF CONTENT */}
                            <Show
                              visible={
                                section.sectionType === ReportSectionTypeName.TableOfContents
                              }
                            >
                              <ReportSectionTableOfContents index={index} />
                            </Show>
                            {/* CONTENT */}
                            <Show visible={section.sectionType === ReportSectionTypeName.Content}>
                              <ReportSectionContent index={index} />
                            </Show>
                            {/* TEXT */}
                            <Show visible={section.sectionType === ReportSectionTypeName.Text}>
                              <ReportSectionText index={index} />
                            </Show>
                            {/* MEDIA ANALYTICS */}
                            <Show
                              visible={section.sectionType === ReportSectionTypeName.MediaAnalytics}
                            >
                              <ReportSectionMediaAnalytics index={index} />
                            </Show>
                            {/* FRONT PAGE IMAGES */}
                            <Show visible={section.sectionType === ReportSectionTypeName.Gallery}>
                              <ReportSectionGallery index={index} />
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
    </styled.ReportEditTemplateForm>
  );
};
