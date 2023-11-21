import { useFormikContext } from 'formik';
import React from 'react';
import { Droppable } from 'react-beautiful-dnd';
import { FaEdit } from 'react-icons/fa';
import { useApp } from 'store/hooks';
import { Col, ContentRow, DraggableContentRow, FormikText, FormikTextArea, Show } from 'tno-core';

import { IReportForm } from '../../interfaces';
import { sortContent } from '../../utils';
import { ContentForm, UserContentForm } from '.';
import { IReportSectionProps } from './ReportSection';

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionContent = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, showForm, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();
    const [{ userInfo }] = useApp();

    const [activeRowIndex, setActiveRowIndex] = React.useState<number>();

    const section = values.sections[index];
    const instance = values.instances.length ? values.instances[0] : null;
    const userId = userInfo?.id ?? 0;
    const sectionContent = values.instances[0].content
      .filter((c) => c.sectionName === section.name)
      .map((c) => ({
        ...c,
        originalIndex: values.instances[0].content.findIndex(
          (oi) => oi.contentId === c.contentId && oi.sectionName === c.sectionName,
        ),
      }));

    const handleRemoveContent = React.useCallback(
      async (index: number) => {
        if (instance) {
          var newItems = [...instance.content];
          newItems.splice(index, 1);
          newItems = newItems.map((c, index) => ({ ...c, sortOrder: index }));
          setActiveRowIndex(undefined);
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
              <div
                className="section-container"
                {...droppableProvided.droppableProps}
                ref={droppableProvided.innerRef}
              >
                {sectionContent.map((ic, sectionIndex) => {
                  // Only display content in this section.
                  // The original index is needed to provide the ability to drag+drop content into other sections.
                  if (ic.content == null) return null;
                  return (
                    <DraggableContentRow
                      key={`${ic.sectionName}-${ic.contentId}-${ic.originalIndex}`}
                      draggableId={`${ic.sectionName}__${ic.contentId}__${ic.originalIndex}`}
                      index={sectionIndex++}
                      row={{
                        content: ic.content,
                        sortOrder: ic.sortOrder,
                        selected: false,
                      }}
                      to={ic.contentId ? `/view/${ic.contentId}` : undefined}
                      onRemove={(content) => {
                        handleRemoveContent(ic.originalIndex);
                      }}
                      onChange={(row) => {
                        const content = instance.content.map((ic) =>
                          ic.contentId !== row.content.id
                            ? ic
                            : {
                                ...ic,
                                sortOrder: row.sortOrder,
                              },
                        );
                        setFieldValue(`instances.0.content`, content);
                      }}
                      onSelected={(content) => {}}
                      showGrip={true}
                      showSortOrder={true}
                      showCheckbox={false}
                      actions={
                        <Show visible={!!ic.contentId}>
                          <FaEdit
                            className="btn btn-link"
                            onClick={() =>
                              activeRowIndex !== ic.originalIndex
                                ? setActiveRowIndex(ic.originalIndex)
                                : setActiveRowIndex(undefined)
                            }
                          />
                        </Show>
                      }
                    >
                      {({ row, ...rest }) => {
                        const headline = row.content.versions?.[userId]?.headline
                          ? row.content.versions[userId].headline ?? ''
                          : row.content.headline;
                        const show =
                          activeRowIndex === ic.originalIndex || row.content.id === 0
                            ? 'all'
                            : showForm
                            ? 'summary'
                            : 'none';
                        return (
                          <>
                            <ContentRow
                              row={{ ...row, content: { ...row.content, headline: headline } }}
                              {...rest}
                            />
                            {row.content.ownerId === userId && row.content.isPrivate ? (
                              <UserContentForm index={ic.originalIndex} show={show} />
                            ) : (
                              <ContentForm index={ic.originalIndex} show={show} />
                            )}
                          </>
                        );
                      }}
                    </DraggableContentRow>
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
