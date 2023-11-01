import { Sentiment } from 'components/sentiment';
import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import { FaGripVertical } from 'react-icons/fa';
import { FaX } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useReportInstances } from 'store/hooks';
import {
  Col,
  FormikText,
  FormikTextArea,
  IReportInstanceContentModel,
  IReportInstanceModel,
  Row,
  Show,
} from 'tno-core';

import { IReportForm } from '../../interfaces';
import { IReportSectionProps } from './ReportSection';

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionContent = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, showForm, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();
    const [{ updateReportInstance }] = useReportInstances();

    const section = values.sections[index];
    const instance = values.instances.length ? values.instances[0] : null;

    const handleRemoveContent = React.useCallback(
      async (instance: IReportInstanceModel, content: IReportInstanceContentModel) => {
        try {
          const result = await updateReportInstance({
            ...instance,
            content: instance.content.filter(
              (c) => c.sectionName !== content.sectionName || c.contentId !== content.contentId,
            ),
          });
          setFieldValue(
            'instances',
            values.instances.map((i) => (i.id === result.id ? result : i)),
          );
          toast.success(`Content removed from folder`);
        } catch {}
      },
      [setFieldValue, updateReportInstance, values.instances],
    );
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
        <Show visible={!!instance?.content.length}>
          <Droppable droppableId={section.name}>
            {(droppableProvided) => (
              <div
                className="section-container"
                {...droppableProvided.droppableProps}
                ref={droppableProvided.innerRef}
              >
                {instance?.content
                  .filter((ic) => ic.sectionName === section.name)
                  .map((ic, index) => {
                    return (
                      <Draggable key={ic.contentId} draggableId={`${ic.contentId}`} index={index}>
                        {(provided) => (
                          <div
                            key={`${section.id}-${index}`}
                            className="content-row"
                            ref={provided.innerRef}
                            {...provided.dragHandleProps}
                            {...provided.draggableProps}
                          >
                            <Row key={`${section.id}-${ic.contentId}`} className="section-content">
                              <div>
                                <FaGripVertical />
                              </div>
                              <div>
                                {ic.content?.byline
                                  ? ic.content.byline
                                  : ic.content?.contributor?.name}
                              </div>
                              <div>{ic.content?.headline}</div>
                              <div>{ic.content?.otherSource}</div>
                              <div>
                                {ic.content?.section}
                                {ic.content?.page ? `:${ic.content.page}` : ''}
                              </div>
                              <div>
                                {ic.content?.publishedOn
                                  ? moment(ic.content?.publishedOn).format('yyyy-MM-DD')
                                  : ''}
                              </div>
                              <div>
                                <Sentiment
                                  value={ic.content?.tonePools?.[0]?.value}
                                  title={`${ic.content?.tonePools?.[0]?.value ?? ''}`}
                                />
                              </div>
                              <div>
                                <FaX
                                  className="btn btn-link error"
                                  onClick={() => handleRemoveContent(instance, ic)}
                                  title="Remove"
                                />
                              </div>
                            </Row>
                          </div>
                        )}
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
  },
);
