import { StartNextReportInfo } from 'features/my-reports/components';
import { IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { moveContent, sortContent, sortReportContent } from 'features/my-reports/utils';
import React from 'react';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
  ResponderProvided,
} from 'react-beautiful-dnd';
import { getDistinct, IReportInstanceModel, OptionItem, ReportSectionTypeName } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import { ReportContentSectionRow } from './sort';
import * as styled from './styled';

export interface IReportEditSortFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** The active row. */
  activeRow?: IReportInstanceContentForm;
  /** Event fires when the content headline is clicked. */
  onContentClick?: (content?: IReportInstanceContentForm, action?: 'previous' | 'next') => void;
}

export const ReportEditSortForm = React.forwardRef<HTMLDivElement | null, IReportEditSortFormProps>(
  ({ disabled, activeRow, onContentClick }, ref) => {
    const { values, setFieldValue } = useReportEditContext();

    const instance = values.instances.length ? values.instances[0] : undefined;

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

    const handleDrop = React.useCallback(
      (result: DropResult, provided: ResponderProvided) => {
        if (instance) {
          const newItems = moveContent(result, instance.content);
          if (newItems) setFieldValue(`instances.0.content`, newItems);
        }
      },
      [instance, setFieldValue],
    );

    if (!instance) return null;

    return (
      <styled.ReportEditSortForm className="report-edit-section" ref={ref}>
        <StartNextReportInfo />
        <DragDropContext onDragEnd={handleDrop}>
          {values.sections
            .filter((section) =>
              [
                ReportSectionTypeName.Content,
                ReportSectionTypeName.Gallery,
                ReportSectionTypeName.MediaAnalytics,
              ].includes(section.sectionType),
            )
            .map((section) => {
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
                .filter((s) =>
                  [
                    ReportSectionTypeName.Content,
                    ReportSectionTypeName.Gallery,
                    ReportSectionTypeName.MediaAnalytics,
                  ].includes(s.sectionType),
                )
                .map((s) => new OptionItem(s.settings.label, s.name));

              return (
                <Droppable key={section.name} droppableId={section.name} isDropDisabled={disabled}>
                  {(droppableProvided) => (
                    <div {...droppableProvided.droppableProps} ref={droppableProvided.innerRef}>
                      {sectionContent.map((row, contentInSectionIndex) => {
                        const isActive =
                          row.sectionName === activeRow?.sectionName &&
                          row.contentId === activeRow?.contentId;

                        return (
                          <Draggable
                            key={`${row.sectionName}-${row.contentId}-${row.originalIndex}`}
                            draggableId={`${row.sectionName}__${row.contentId}__${row.originalIndex}`}
                            index={contentInSectionIndex}
                            isDragDisabled={disabled}
                          >
                            {(draggable) => {
                              return (
                                <div
                                  ref={draggable.innerRef}
                                  className={isActive ? 'active-content' : ''}
                                  {...draggable.dragHandleProps}
                                  {...draggable.draggableProps}
                                >
                                  <ReportContentSectionRow
                                    key={`${row.instanceId}-${row.sectionName}-${row.contentId}`}
                                    contentIndex={contentInSectionIndex}
                                    row={row}
                                    disabled={disabled}
                                    showSelectSection
                                    showSortOrder
                                    sectionOptions={sectionOptions}
                                    onContentClick={onContentClick}
                                    onRemove={(index) => handleRemoveContent(index)}
                                    onChangeSection={(sectionName, row) => {
                                      handleChangeSection(sectionName, row, instance);
                                    }}
                                    onBlurSortOrder={(row) => handleChangeSortOrder(row, instance)}
                                  />
                                </div>
                              );
                            }}
                          </Draggable>
                        );
                      })}
                    </div>
                  )}
                </Droppable>
              );
            })}
        </DragDropContext>
      </styled.ReportEditSortForm>
    );
  },
);
