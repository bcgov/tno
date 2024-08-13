import { Action } from 'components/action';
import { Modal } from 'components/modal';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
import {
  createReportInstanceContent,
  sortContent,
  sortReportContent,
} from 'features/my-reports/utils';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import { FaArrowsRotate, FaPlus } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useLookup } from 'store/hooks';
import {
  Col,
  FormikText,
  FormikWysiwyg,
  getDistinct,
  IReportInstanceModel,
  OptionItem,
  ReportSectionTypeName,
  Row,
  Settings,
  Show,
  useModal,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import { ReportContentSectionRow } from './ReportContentSectionRow';

export interface IReportSectionContentProps extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  sectionIndex: number;
  /** Icon to display in header */
  icon?: React.ReactNode;
  /** Enable toggling the form values */
  showForm?: boolean;
  /** Show the section content. */
  showContent?: boolean;
  /** Whether to show the add story row */
  showAdd?: boolean;
  /** Form is disabled. */
  disabled?: boolean;
  /** The active row. */
  activeRow?: IReportInstanceContentForm;
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
export const ReportSectionContent: React.FC<IReportSectionContentProps> = ({
  sectionIndex,
  showForm,
  showContent = true,
  showAdd,
  disabled,
  activeRow,
  onContentClick,
  updateForm,
}) => {
  const [{ userInfo }] = useApp();
  const { values, isSubmitting, setFieldValue, onRegenerateSection, setSubmitting } =
    useReportEditContext();
  const [{ isReady, settings }] = useLookup();
  const { isShowing, toggle } = useModal();

  const [defaultLicenseId, setDefaultLicenseId] = React.useState(0);
  const [defaultMediaTypeId, setDefaultMediaTypeId] = React.useState(0);

  const userId = userInfo?.id ?? 0;
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

  React.useEffect(() => {
    if (isReady) {
      const defaultLicenseId = settings.find(
        (s) => s.name === Settings.DefaultSubscriberContentLicense,
      )?.value;
      if (defaultLicenseId) setDefaultLicenseId(+defaultLicenseId);
      else
        toast.error(
          `Configuration settings '${Settings.DefaultSubscriberContentLicense}' is required.`,
        );
    }
  }, [isReady, settings]);

  React.useEffect(() => {
    if (isReady) {
      const defaultMediaTypeId = settings.find(
        (s) => s.name === Settings.DefaultSubscriberContentMediaType,
      )?.value;
      if (defaultMediaTypeId) setDefaultMediaTypeId(+defaultMediaTypeId);
      else
        toast.error(
          `Configuration settings '${Settings.DefaultSubscriberContentMediaType}' is required.`,
        );
    }
  }, [isReady, settings]);

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

  const addStory = React.useCallback(
    (instanceId: number | undefined, sectionName: string) => {
      if (instanceId) {
        const newContent = createReportInstanceContent(
          instanceId,
          sectionName,
          userId,
          defaultLicenseId,
          defaultMediaTypeId,
        );
        onContentClick?.(newContent);
      }
    },
    [defaultLicenseId, defaultMediaTypeId, onContentClick, userId],
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
      <Droppable droppableId={section.name} isDropDisabled={disabled}>
        {(droppableProvided) => (
          <div {...droppableProvided.droppableProps} ref={droppableProvided.innerRef}>
            {showAdd && !disabled && (
              <Row className="add-story">
                <Col flex="1"></Col>
                <Col flex="1">
                  <Action
                    icon={<FaPlus />}
                    label="Create a story"
                    onClick={() => addStory(instance.id, section.name)}
                  />
                </Col>
                {!!section.id && !disabled && (section.filterId || section.folderId) ? (
                  <Col flex="1">
                    <Row justifyContent="flex-end">
                      <Action
                        className="icon-refresh"
                        icon={<FaArrowsRotate />}
                        label="Regenerate section"
                        disabled={isSubmitting}
                        onClick={(e) => toggle()}
                        direction="row-reverse"
                      />
                    </Row>
                  </Col>
                ) : (
                  <Col flex="1"></Col>
                )}
              </Row>
            )}
            <Show visible={!sectionContent.length}>
              <Row justifyContent="center">Drop content here</Row>
            </Show>
            {showContent ? (
              sectionContent.map((ic, contentInSectionIndex) => {
                // Only display content in this section.
                // The original index is needed to provide the ability to drag+drop content into other sections.
                if (ic.content == null) return null;
                const isActive =
                  activeRow?.sectionName === section.name && activeRow?.contentId === ic.contentId;
                const isSame = activeRow?.contentId === ic.contentId;

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
                          className={`${isSame ? 'active-content ' : ''}${
                            isActive ? 'active-row' : ''
                          }`}
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
              })
            ) : (
              <></>
            )}
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
