import { IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import { Col, FormikText, FormikTextArea, Show } from 'tno-core';

import { IReportForm } from '../../interfaces';
import { sortContent } from '../../utils';
import { IReportSectionProps } from '../old/components';
import { ReportContentSectionRow } from './ReportContentSectionRow';

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportContentSection: React.FC<IReportSectionProps> = ({
  index,
  showForm,
  ...rest
}) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();

  const section = values.sections[index];
  const instance = values.instances.length ? values.instances[0] : null;
  const sectionContent =
    instance?.content
      .filter((c) => c.sectionName === section.name)
      .map(
        (c) =>
          ({
            ...c,
            originalIndex: instance.content.findIndex(
              (oi) => oi.contentId === c.contentId && oi.sectionName === c.sectionName,
            ),
          } as IReportInstanceContentForm),
      ) ?? [];

  const handleRemoveContent = React.useCallback(
    async (index: number) => {
      if (instance) {
        var newItems = [...instance.content];
        newItems.splice(index, 1);
        newItems = newItems.map((c, index) => ({ ...c, sortOrder: index }));
        setFieldValue('instances.0.content', sortContent(newItems));
      }
    },
    [instance, setFieldValue],
  );

  if (instance == null) return null;

  return (
    <Col gap="0.5rem">
      <Show visible={showForm}>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikTextArea name={`sections.${index}.description`} label="Summary text:" />
      </Show>
      <Show visible={!!section.filterId && !instance?.content.length}>
        <p>No content was returned by the filter.</p>
      </Show>
      <Show visible={!!section.folder && !instance?.content.length}>
        <p>Folder is empty.</p>
      </Show>
      <Show visible={!!instance.content.length}>
        <Droppable droppableId={section.name}>
          {(droppableProvided) => (
            <div {...droppableProvided.droppableProps} ref={droppableProvided.innerRef}>
              {sectionContent.map((ic, contentInSectionIndex) => {
                // Only display content in this section.
                // The original index is needed to provide the ability to drag+drop content into other sections.
                if (ic.content == null) return null;
                return (
                  <Draggable
                    key={`${ic.sectionName}-${ic.contentId}-${ic.originalIndex}`}
                    draggableId={`${ic.sectionName}__${ic.contentId}__${ic.originalIndex}`}
                    index={contentInSectionIndex}
                  >
                    {(draggable) => {
                      if (!ic.content) return <></>;

                      return (
                        <div
                          ref={draggable.innerRef}
                          {...draggable.dragHandleProps}
                          {...draggable.draggableProps}
                        >
                          <ReportContentSectionRow
                            row={ic}
                            index={contentInSectionIndex}
                            show={!ic.contentId ? 'all' : 'none'}
                            onRemove={(index) => handleRemoveContent(index)}
                          />
                        </div>
                      );
                    }}
                  </Draggable>
                );
              })}
              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      </Show>
    </Col>
  );
};
