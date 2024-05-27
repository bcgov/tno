import { Action } from 'components/action';
import { Modal } from 'components/modal';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { sortContent, sortReportContent } from 'features/my-reports/utils';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import { FaArrowsSpin } from 'react-icons/fa6';
import {
  Col,
  FormikText,
  FormikWysiwyg,
  getDistinct,
  IReportInstanceModel,
  OptionItem,
  ReportSectionTypeName,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import { ReportContentSectionRow } from './ReportContentSectionRow';

export interface IReportSectionGalleryProps extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  sectionIndex: number;
  /** Icon to display in header */
  icon?: React.ReactNode;
  /** Enable toggling the form values */
  showForm?: boolean;
  /** Form is disabled. */
  disabled?: boolean;
  /** Event fires when the row is clicked. */
  onContentClick?: (content: IReportInstanceContentForm) => void;
  /** Event to update the original report. */
  updateForm: (values: IReportForm) => void;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionGallery: React.FC<IReportSectionGalleryProps> = ({
  sectionIndex,
  showForm,
  disabled,
  onContentClick,
  updateForm,
  ...rest
}) => {
  const { values, isSubmitting, setFieldValue, onRegenerateSection, setSubmitting } =
    useReportEditContext();
  const { isShowing, toggle } = useModal();

  const section = values.sections[sectionIndex];
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
  const sectionOptions = values.sections
    .filter((s) => s.sectionType === ReportSectionTypeName.Content)
    .map((s) => new OptionItem(s.settings.label, s.name));

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

  const handleChangeSection = React.useCallback(
    (sectionName: string, row: IReportInstanceContentForm, instance: IReportInstanceModel) => {
      // Move the content to the specified section.
      // Remove duplicates.
      const content = getDistinct(
        instance.content.map((c) =>
          c.contentId === row.contentId && c.sectionName === row.sectionName
            ? { ...row, sectionName, sortOrder: -1 }
            : c,
        ),
        (c) => `${c.contentId}-${c.sectionName}`,
      );
      setFieldValue('instances.0.content', sortReportContent(values, content));
    },
    [setFieldValue, values],
  );

  const handleChangeSortOrder = React.useCallback(
    (row: IReportInstanceContentForm, instance: IReportInstanceModel) => {
      const content = instance.content.map((c) =>
        c.contentId === row.contentId && c.sectionName === row.sectionName ? row : c,
      );
      setFieldValue(`instances.0.content`, sortReportContent(values, content));
    },
    [setFieldValue, values],
  );

  if (instance == null) return null;

  return (
    <Col gap="0.5rem">
      <Show visible={showForm}>
        <FormikText
          name={`sections.${sectionIndex}.settings.label`}
          label="Section heading:"
          disabled={disabled}
        />
        <FormikWysiwyg
          name={`sections.${sectionIndex}.description`}
          label="Summary text:"
          disabled={disabled}
        />
      </Show>
      {!!section.id && !disabled && (section.filterId || section.folderId) && (
        <Col flex="1">
          <Row justifyContent="flex-end">
            <Action
              icon={<FaArrowsSpin />}
              label="Regenerate section"
              disabled={isSubmitting}
              onClick={(e) => toggle()}
              direction="row-reverse"
            />
          </Row>
        </Col>
      )}
      <Droppable droppableId={section.name} isDropDisabled={disabled}>
        {(droppableProvided) => (
          <div {...droppableProvided.droppableProps} ref={droppableProvided.innerRef}>
            <Show visible={!sectionContent.length}>
              <Row justifyContent="center">Drop content here</Row>
            </Show>
            {sectionContent.map((ic, contentInSectionIndex) => {
              // Only display content in this section.
              // The original index is needed to provide the ability to drag+drop content into other sections.
              if (ic.content == null) return null;
              return (
                <Draggable
                  key={`${ic.sectionName}-${ic.contentId}-${ic.originalIndex}`}
                  draggableId={`${ic.sectionName}__${ic.contentId}__${ic.originalIndex}`}
                  index={contentInSectionIndex}
                  isDragDisabled={disabled}
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
                          disabled={disabled}
                          row={ic}
                          contentIndex={contentInSectionIndex}
                          show={!ic.contentId ? 'all' : 'none'}
                          onRemove={(index) => handleRemoveContent(index)}
                          showSelectSection={false}
                          sectionOptions={sectionOptions}
                          onChangeSection={(sectionName, row) => {
                            handleChangeSection(sectionName, row, instance);
                          }}
                          showSortOrder
                          onBlurSortOrder={(row) => handleChangeSortOrder(row, instance)}
                          onContentClick={onContentClick}
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
      <Show visible={!!section.filterId && !sectionContent.length}>
        <p>No content was returned by the filter, or duplicate content was removed.</p>
      </Show>
      <Show visible={!!section.folderId && !sectionContent.length}>
        <p>Folder is empty, or duplicate content was removed.</p>
      </Show>
      <Show visible={!section.filterId && !section.folderId && !sectionContent.length}>
        <p>Section has no data source configured.</p>
      </Show>
      <Modal
        headerText="Regenerate Section"
        body={
          <>
            <p>
              Regenerating this section will remove content and then rerun the data source to
              populate with content.
            </p>
            <p>
              This process will not update other sections. As such report content options that
              remove duplicates in subsequent sections will not be applied.
            </p>
            <p>Do you want to proceed?</p>
          </>
        }
        isShowing={isShowing}
        hide={toggle}
        type="default"
        isSubmitting={isSubmitting}
        confirmText="Yes, Regenerate It"
        onConfirm={async () => {
          try {
            setSubmitting(true);
            const form = await onRegenerateSection(values, section.id);
            if (form) updateForm(form);
          } finally {
            setSubmitting(false);
            toggle();
          }
        }}
      />
    </Col>
  );
};
