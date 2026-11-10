import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import { FaArrowsRotate } from 'react-icons/fa6';
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

import { Modal } from '@/subscriber/components/modal';
import { RefreshButton } from '@/subscriber/components/refresh-button';
import { IReportInstanceContentForm } from '@/subscriber/features/my-reports/interfaces';
import { sortContent, sortReportContent } from '@/subscriber/features/my-reports/utils';
import { useReportInstances } from '@/subscriber/store/hooks';
import { IReportInstanceChartModel } from '@/subscriber/store/hooks/subscriber/interfaces';

import { useReportEditContext } from '../../ReportEditContext';
import { isAggregateChart } from '../../settings/template/utils';
import { ReportContentSectionRow } from './ReportContentSectionRow';

export interface IReportSectionMediaAnalyticsProps extends React.AllHTMLAttributes<HTMLDivElement> {
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
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionMediaAnalytics: React.FC<IReportSectionMediaAnalyticsProps> = ({
  sectionIndex,
  showForm,
  disabled,
  onContentClick,
  ...rest
}) => {
  const { values, isSubmitting, setFieldValue, onRegenerateSection } = useReportEditContext();
  const { isShowing, toggle } = useModal();
  const [{ getReportInstanceCharts }] = useReportInstances();
  const [charts, setCharts] = React.useState<IReportInstanceChartModel[]>([]);

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
          }) as IReportInstanceContentForm,
      ) ?? [];
  // These sections fetch no stories, their charts count them instead.
  const analyticsOnly =
    !section.settings.useAllContent &&
    !section.linkedReportId &&
    (!!section.filterId || !!section.folderId) &&
    section.chartTemplates.length > 0 &&
    section.chartTemplates.every(isAggregateChart);
  const instanceId = instance?.id;

  React.useEffect(() => {
    if (!analyticsOnly || !instanceId) return;
    let cancelled = false;
    getReportInstanceCharts(instanceId)
      .then((results) => {
        if (!cancelled) setCharts(results.filter((c) => c.sectionId === section.id));
      })
      .catch(() => {
        if (!cancelled) setCharts([]);
      });
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [analyticsOnly, instanceId, section.id]);

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
            <RefreshButton
              icon={<FaArrowsRotate />}
              label="Regenerate section"
              disabled={isSubmitting}
              onClick={() => toggle()}
              direction="row-reverse"
            />
          </Row>
        </Col>
      )}
      <Show visible={analyticsOnly}>
        <Col gap="0.5rem" className="analytics-summary">
          {section.chartTemplates.map((chart) => {
            const row = charts.find((c) => c.chartTemplateId === chart.id);
            return (
              <Row key={chart.id} gap="1rem" alignItems="center">
                <b>{chart.name}</b>
                <span>
                  {row
                    ? `${row.docCount.toLocaleString()} stories counted, ${new Date(
                        row.generatedOn,
                      ).toLocaleString()}`
                    : 'Not generated yet'}
                </span>
                {row?.isTruncated && <span className="info">Truncated</span>}
              </Row>
            );
          })}
          <p>Charts refresh when the report is regenerated.</p>
        </Col>
      </Show>
      <Show visible={!!instance.content.length && !analyticsOnly}>
        <Droppable droppableId={section.name} isDropDisabled={disabled}>
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
                    isDragDisabled={disabled}
                  >
                    {(draggable) => {
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
      </Show>
      <Show visible={!analyticsOnly && !!section.filterId && !sectionContent.length}>
        <p>No content was returned by the filter, or duplicate content was removed.</p>
      </Show>
      <Show visible={!analyticsOnly && !!section.folderId && !sectionContent.length}>
        <p>Folder is empty, or duplicate content was removed.</p>
      </Show>
      <Show visible={section.settings.useAllContent}>
        <p>This section will use content from all other sections.</p>
      </Show>
      <Show visible={!!section.linkedReportId}>
        <p>This section will receive content from a linked report.</p>
      </Show>
      <Show
        visible={
          !section.filterId &&
          !section.folderId &&
          !section.linkedReportId &&
          !section.settings.useAllContent &&
          !sectionContent.length
        }
      >
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
        onClose={toggle}
        type="default"
        isSubmitting={isSubmitting}
        confirmText="Yes, Regenerate It"
        onConfirm={async () => {
          try {
            await onRegenerateSection(values, section.id);
          } finally {
            toggle();
          }
        }}
      />
    </Col>
  );
};
