import { Button } from 'components/button';
import { PageSection } from 'components/section';
import { IReportForm, IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { toForm } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaCloud } from 'react-icons/fa6';
import { useApp, useContent, useReports } from 'store/hooks';
import { Col, Row, Show } from 'tno-core';

import { ContentForm } from './ContentForm';
import { UserContentForm } from './UserContentForm';

export interface IContentEditFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** The current row being edited. */
  row?: IReportInstanceContentForm;
  /** Event fires when the update button is clicked and performs an update to the API. */
  onUpdate?: (row?: IReportInstanceContentForm) => void;
}

/**
 * Provides a form to edit content in a report.
 * Handles editor content and custom subscriber content.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentEditForm = ({ disabled, row: initialRow, onUpdate }: IContentEditFormProps) => {
  const [{ userInfo }] = useApp();
  const { values, setSubmitting, isSubmitting, setValues } = useFormikContext<IReportForm>();
  const [, { updateReport }] = useReports();
  const [, { addContent, updateContent }] = useContent();

  const [loading, setLoading] = React.useState(false);
  const [form, setForm] = React.useState(initialRow);

  const userId = userInfo?.id ?? 0;

  React.useEffect(() => {
    setForm(initialRow);
  }, [initialRow]);

  const handleAddUpdateContent = React.useCallback(
    async (values: IReportForm, row: IReportInstanceContentForm) => {
      try {
        setLoading(true);
        const content = row.content;

        if (!content) return null;

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
            return toForm(reportResult);
          } else {
            return {
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
            };
          }
        }
      } catch {
      } finally {
        setLoading(false);
      }
    },
    [addContent, onUpdate, updateContent, updateReport],
  );

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
                  onClick={() => onUpdate?.(undefined)}
                  disabled={isSubmitting || disabled}
                  variant="secondary"
                >
                  Cancel
                </Button>
                <Button
                  onClick={async () => {
                    // Save the updated story and then apply the results to the report.
                    if (form.content) {
                      try {
                        setSubmitting(true);
                        const result = await handleAddUpdateContent(values, form);
                        if (result) setValues(result);
                      } catch {
                      } finally {
                        setSubmitting(false);
                      }
                    }
                  }}
                  disabled={isSubmitting || disabled}
                >
                  Save edits
                  <FaCloud />
                </Button>
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
          content={form}
          show={'all'}
          onContentChange={(content) => {
            setForm({ ...content });
          }}
          loading={loading}
        />
      ) : (
        <ContentForm
          content={form.content}
          show={'all'}
          onContentChange={(content) => {
            setForm({ ...form, content });
          }}
          loading={loading}
        />
      )}
    </PageSection>
  );
};
