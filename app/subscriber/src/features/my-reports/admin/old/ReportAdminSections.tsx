import { Box } from 'components/box';
import { useFormikContext } from 'formik';
import React from 'react';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
  ResponderProvided,
} from 'react-beautiful-dnd';
import { FaPlusCircle } from 'react-icons/fa';
import { Button, Col, ReportSectionTypeName, Row } from 'tno-core';

import { defaultReportSection } from '../../constants';
import { IReportForm } from '../../interfaces';
import { ReportSection } from './components';

/**
 * Component provides a way to manage all the sections within a report.
 * @returns Component
 */
export const ReportAdminSections: React.FC = () => {
  const { values, setFieldValue, isSubmitting } = useFormikContext<IReportForm>();

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
      <Box title="Add / Edit Sections">
        <p>Add blocks to create a sectioned layout for your report.</p>
        <p>Drag blocks to reorder them.</p>
        <p>
          <b>Table of Contents</b> - An ordered list of stories in the section.
        </p>
        <p>
          <b>Text block</b> - Creates a section for you to compose paragraph text as an intro or
          summary.
        </p>
        <p>
          <b>Stories block</b> - Creates a section that will populate with stories dynamically from
          the saved searches and/or folders you choose.
        </p>
        <p>
          <b>Media Analytics block</b> - Creates a section that will show media data in charts.
        </p>
        <Row gap="1rem">
          <Row gap="1rem">
            <Button
              onClick={() => addSection(ReportSectionTypeName.TableOfContents)}
              disabled={isSubmitting}
            >
              <Row alignItems="center" gap="0.5rem">
                <FaPlusCircle />
                Table of Contents
              </Row>
            </Button>
            <Button
              onClick={() => addSection(ReportSectionTypeName.Summary)}
              disabled={isSubmitting}
            >
              <Row alignItems="center" gap="0.5rem">
                <FaPlusCircle />
                Text
              </Row>
            </Button>
          </Row>
          <Row gap="1rem">
            <Button
              onClick={() => addSection(ReportSectionTypeName.Content, false, false, true)}
              disabled={isSubmitting}
            >
              <Row alignItems="center" gap="0.5rem">
                <FaPlusCircle />
                Media
              </Row>
            </Button>
            <Button
              onClick={() => addSection(ReportSectionTypeName.Content, true, false, false)}
              disabled={isSubmitting}
            >
              <Row alignItems="center" gap="0.5rem">
                <FaPlusCircle />
                Media Analytics
              </Row>
            </Button>
          </Row>
        </Row>
      </Box>
      <Col className="report-sections">
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
                          <ReportSection index={index} />
                        </div>
                      )}
                    </Draggable>
                  );
                })}
              </div>
            )}
          </Droppable>
        </DragDropContext>
      </Col>
    </Col>
  );
};
