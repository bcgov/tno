import { Action } from 'components/action';
import { Button } from 'components/button';
import { PageSection } from 'components/section';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { IContentValidationErrors } from 'features/my-reports/interfaces/IContentValidationErrors';
import { toForm } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowLeft, FaArrowRight, FaCloud } from 'react-icons/fa6';
import { useApp, useContent, useReports } from 'store/hooks';
import { Col, IContentModel, Row, Show } from 'tno-core';

import { ContentForm } from './ContentForm';
import { UserContentForm } from './UserContentForm';

export interface IContentEditFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** The current row being edited. */
  row?: IReportInstanceContentForm;
  /** Event fires when the update button is clicked and performs an update to the API. */
  onUpdate?: (row?: IReportInstanceContentForm) => void;
  /** Event fires when user clicks previous/next buttons */
  onNavigate?: (action: 'previous' | 'next') => void;
}

/**
 * Provides a form to edit content in a report.
 * Handles editor content and custom subscriber content.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentEditForm = ({
  disabled,
  row: initialRow,
  onUpdate,
  onNavigate,
}: IContentEditFormProps) => {
  const [{ userInfo }] = useApp();
  const { values, setSubmitting, isSubmitting, setValues } = useFormikContext<IReportForm>();
  const [, { updateReport }] = useReports();
  const [, { addContent, updateContent }] = useContent();

  const [form, setForm] = React.useState(initialRow);

  const [errors, setErrors] = React.useState<IContentValidationErrors>();

  const userId = userInfo?.id ?? 0;

  React.useEffect(() => {
    setForm(initialRow);
  }, [initialRow]);

  const validate = (values: IContentModel) => {
    const err: IContentValidationErrors = { hasErrors: false };
    if (!values.headline) {
      err.headline = 'Headline is Required.';
      err.hasErrors = true;
    }
    if (!values.otherSource) {
      err.source = 'Source is Required.';
      err.hasErrors = true;
    }
    if (!values.publishedOn) {
      err.publishedOn = 'Published On is Required.';
      err.hasErrors = true;
    }
    setErrors(err);
    return err;
  };

  const handleAddUpdateContent = React.useCallback(
    async (values: IReportForm, row: IReportInstanceContentForm) => {
      try {
        setSubmitting(true);
        const content = row.content;
        if (!content) return null;
        const err = validate(content);
        if (err.hasErrors) return null;
        const originalId = content.id;
        const contentResult = !content.id
          ? await addContent(content)
          : await updateContent(content);
        if (contentResult) {
          const instanceContent: IReportInstanceContentForm = {
            contentId: contentResult.id,
            content: contentResult,
            instanceId: row.instanceId,
            sectionName: row.sectionName,
            sortOrder: 0,
            originalIndex: row.originalIndex,
          };
          setForm(instanceContent);
          onUpdate?.(instanceContent);
          if (!originalId) {
            // Added content needs to update the report instance.
            const instance = values.instances.length ? values.instances[0] : undefined;
            if (!instance) return null;

            // Resort section and place new content at the beginning.
            const sectionIndex = instance.content.findIndex(
              (c) => c.sectionName === row.sectionName,
            );
            if (sectionIndex === -1) return null;

            instance.content.splice(sectionIndex, 0, instanceContent);
            let contentIndex = 0;

            const updatedReport = {
              ...values,
              instances: values.instances.map((instance) => ({
                ...instance,
                content: instance.content.map((c, i) => {
                  if (c.sectionName === row.sectionName) {
                    return { ...c, sortOrder: contentIndex++ };
                  }
                  return c;
                }),
              })),
            };

            // Update the report instances with the latest content.
            const reportResult = await updateReport(updatedReport, true);
            if (reportResult) setValues(toForm(reportResult));
          } else {
            setValues({
              ...values,
              instances: values.instances.map((instance, index) =>
                index === 0
                  ? {
                      ...instance,
                      content: instance.content.map((c) =>
                        c.contentId === contentResult.id ? { ...c, content: contentResult } : c,
                      ),
                    }
                  : instance,
              ),
            });
          }
        }
      } catch {
      } finally {
        setSubmitting(false);
      }
    },
    [addContent, onUpdate, setSubmitting, setValues, updateContent, updateReport],
  );

  const handleKeyDown = React.useCallback(
    (e: KeyboardEvent) => {
      if (!form?.content) return;
      if (e.code === 'Escape') onUpdate?.();
      // added metaKey support for MAC
      // allows use of Command key as well as Control key for windows
      else if (e.ctrlKey || e.metaKey) {
        if (e.code === 'Enter') handleAddUpdateContent(values, form);
        else if (e.code === 'ArrowUp' || e.code === 'ArrowLeft') {
          console.log('NAV PREV');
          onNavigate?.('previous');
          e.stopImmediatePropagation();
          e.preventDefault();
        } else if (e.code === 'ArrowDown' || e.code === 'ArrowRight') {
          console.log('NAV NEXT');
          onNavigate?.('next');
          e.stopImmediatePropagation();
          e.preventDefault();
        }
      }
    },
    [form, handleAddUpdateContent, onNavigate, onUpdate, values],
  );

  // refresh the window event listener anytime the handler changes due to content changes
  React.useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [handleKeyDown]);

  if (!form?.content) return <></>;

  return (
    <PageSection
      header={
        <Col flex="1">
          <Row flex="1" alignItems="center" gap="1rem">
            <Col flex="1" gap="0.5rem">
              <Row alignItems="center">
                <label>Story Preview</label>
              </Row>
            </Col>
            <Col gap="0.5rem">
              <Row gap="1rem" justifyContent="flex-end">
                <Button
                  onClick={() => onUpdate?.()}
                  disabled={isSubmitting || disabled}
                  variant="secondary"
                >
                  Cancel
                </Button>
                <Button
                  onClick={() => {
                    handleAddUpdateContent(values, form);
                  }}
                  disabled={isSubmitting || disabled}
                >
                  Save edits
                  <FaCloud />
                </Button>
                <Action
                  icon={<FaArrowLeft size="20" />}
                  title="Previous"
                  onClick={() => onNavigate?.('previous')}
                />
                <Action icon={<FaArrowRight />} title="Next" onClick={() => onNavigate?.('next')} />
              </Row>
            </Col>
          </Row>
          <Row className="sub-title">
            <Show visible={!!form.contentId}>
              <Col>
                <label className="h2">Editing this story:</label>
                <p>Any changes made to the headline or story will be reflected in your reports.</p>
              </Col>
            </Show>
            <Show visible={!form.contentId}>
              <Col>
                <label className="h2">Add this story:</label>
              </Col>
            </Show>
          </Row>
        </Col>
      }
    >
      {form.content?.ownerId === userId && form.content?.isPrivate ? (
        <UserContentForm
          errors={errors}
          content={form}
          show={'all'}
          onContentChange={(content) => {
            setForm({ ...content });
          }}
          loading={isSubmitting}
        />
      ) : (
        <ContentForm
          content={form.content}
          show={'all'}
          onContentChange={(content) => {
            setForm({ ...form, content });
          }}
          loading={isSubmitting}
        />
      )}
      <span></span>
    </PageSection>
  );
};
